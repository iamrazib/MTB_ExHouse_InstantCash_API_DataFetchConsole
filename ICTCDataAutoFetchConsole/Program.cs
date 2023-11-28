using ICTCDataAutoFetchConsole.MTBRemittanceService;
using ICTCDataAutoFetchConsole.CoreMiddlewareService;
using ICTCDataAutoFetchConsole.ICTCServiceClient;
//using ICTCDataAutoFetchConsole.UATRemitServiceReference;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Net;

namespace ICTCDataAutoFetchConsole
{
    /*
     * Author: Sk. Razibul Islam
     * ----31.12.2020 - balance check SKIPPED as sometimes CBS balance calculate problem.
     * 17.05.2021 --- changing
     * 20.12.2021 --- BEFTN mode open
     * 29.12.2021 --- core middleware lines open
     * 02.01.2022 --- balance check added, skip sending X to IC system
     * 01.06.2022 --- beneficiary & sender name upto 99 char
     */

    class Program
    {   
        //API LIVE
        static RemitServiceSoapClient remitServiceClient = new RemitServiceSoapClient();
        static CTCServiceClient ictcclient = new ICTCServiceClient.CTCServiceClient();

        //API UAT
        //static RemitServiceSoapClient remitServiceClient = new RemitServiceSoapClient();

        static Manager mg = new Manager();
        static string passwd = "";

        static string downloadBranch = Definitions.AccessCode.Values.downloadBranch;
        static string downloadUser = Definitions.AccessCode.Values.downloadUser;
        static string userId = Definitions.AccessCode.Values.ICTCUserId;
        static bool IS_INSERT_TO_LOG_TABLE = true;


        static void Main(string[] args)
        {

            //ManuallyConfirmDownloadAccountCreditTxnTable("201977788");
            //ManuallyConfirmDownloadAccountCreditTxnTable("199546045");
            //RejectTxnAtICTCEndDueToPaymentFail(userId, "199862533", "Invalid Account, This account cannot accept funds.");
            //RejectTxnAtICTCEndDueToPaymentFail(userId, "199616767", "Inactive Account status");
            //ManuallyConfirmAccountCreditTxnTable("201977788");


            while (true)
            {
                //----- DOWNLOAD
                Console.WriteLine("ICTC Starting DownloadAccountTxn... -->" + DateTime.Now);

                DownloadAccountTxn();

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "End DownloadAccountCreditTxn..."); }
                Console.WriteLine("ICTC End DownloadAccountCreditTxn... -->" + DateTime.Now);
                Thread.Sleep(5000); //wait 5 sec


                //----- MTB AC CREDIT
                Console.WriteLine();

                Console.WriteLine("ICTC Starting ProcessOwnAccountCreditTxn... -->" + DateTime.Now);
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "Starting ProcessOwnAccountCreditTxn..."); }

                ProcessOwnAccountCreditTxn();

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "End ProcessOwnAccountCreditTxn..."); }
                Console.WriteLine("ICTC End ProcessOwnAccountCreditTxn... -->" + DateTime.Now);
                
                
                Thread.Sleep(5000); //wait 5 sec

                //----- BEFTN
                Console.WriteLine();
                Console.WriteLine("ICTC Starting ProcessBEFTNTxn... -->" + DateTime.Now);
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "Starting ProcessBEFTNTxn..."); }

                UploadBEFTNTxnIntoSystem();

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "End ProcessBEFTNTxn..."); }
                Console.WriteLine("ICTC End ProcessBEFTNTxn... -->" + DateTime.Now);
                

                int sleepTime = 300000;

                Console.WriteLine();
                Console.WriteLine("ICTC --> Going to wait " + (sleepTime / 1000) + " seconds..." + DateTime.Now);
                Console.WriteLine();
                Thread.Sleep(sleepTime); // wait 5 min
            }

        }

        private static void ManuallyConfirmAccountCreditTxnTable(string refNo)
        {
            try
            {
                ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "Y", "");

                if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
                {
                    mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "Y", "");
                    Console.WriteLine("ICTC_Number -> " + refNo + " , MTB AccountCredit Txn PAID OK.");                                        
                }
            }
            catch (Exception ex)
            {                
            }
        }

        private static void ManuallyConfirmDownloadAccountCreditTxnTable(string refNo)
        {
            int confirmCount = 0;
            ConfirmTranResponse confirmTranResp = new ConfirmTranResponse();

            confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "D", "");
            if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
            {
                confirmCount++;
                mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "D", "");
                Console.WriteLine("ICTC_Number -> " + refNo + "  Downloaded OK.");
            }
        }

        private static void RejectTxnAtICTCEndDueToPaymentFail(string userId, string refNo, string msgValue)
        {
            try
            {
                ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "X", msgValue);

                mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "X", msgValue);
                Console.WriteLine("ICTC_Number -> " + refNo + "  " + msgValue);

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectTxnAtICTCEndDueToPaymentFail", "RefNo=" + refNo + ", Txn Update at DB Complete.."); }
            }
            catch (Exception ex)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectTxnAtICTCEndDueToPaymentFail", "RefNo=" + refNo + ", RejectTxnAtICTCEndDueToPaymentFail Error: " + ex); }
            }
        }

        private static void UploadBEFTNTxnIntoSystem()
        {
            string refNo, autoIDictc, msgCodeValue = "", txnStatus = "";
            string exhUserId, beneficiaryAccountNo, beneficiaryName, bankName, branchName, routingNumber, beneficiaryAddress, senderName, senderAddress, transferCurrency, paymentDescription;
            decimal receivingAmount;
            int partyId;

            XmlDocument xDoc = new XmlDocument();
            XmlNodeList msgCode;
            XmlNodeList msgVal;

            string frmDate = DateTime.Now.ToString("dd-MMM-yyyy");
            DataTable dtIctcBeftn = GetICTCBEFTNAccData(frmDate, frmDate);

            if (IS_INSERT_TO_LOG_TABLE)
            { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "Date: " + frmDate + ", BEFTNTxn Row Count=" + dtIctcBeftn.Rows.Count); }

            if (dtIctcBeftn.Rows.Count > 0)
            {
                for (int rCnt = 0; rCnt < dtIctcBeftn.Rows.Count; rCnt++)
                {
                    txnStatus = dtIctcBeftn.Rows[rCnt]["TXN_STATUS"].ToString();

                    if (!txnStatus.Equals("") && txnStatus.Equals("RECEIVED"))
                    {
                        refNo = dtIctcBeftn.Rows[rCnt]["ICTC_NUMBER"].ToString();
                        DataTable dtBeftnRemitInfo = mg.GetBeftnRemitInfo(userId, refNo);
                        if (dtBeftnRemitInfo.Rows.Count > 0)
                        {
                            if (IS_INSERT_TO_LOG_TABLE)
                            { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "Error: RefNo=" + refNo + ", ALready exists in BEFTNRequest table, Need to Update ICTC table status"); }
                        }
                        else
                        {
                            msgCodeValue = "";
                            autoIDictc = dtIctcBeftn.Rows[rCnt]["AutoId"].ToString();
                            partyId = Definitions.AccessCode.Values.ICTCPartyID;

                            DataTable dtExchAccInfo = mg.GetExchangeHouseAccountNo(userId, partyId.ToString());
                            exhUserId = dtExchAccInfo.Rows[0]["UserId"].ToString();
                            passwd = Definitions.AccessCode.Values.ICTCPassword;

                            refNo = dtIctcBeftn.Rows[rCnt]["ICTC_NUMBER"].ToString();
                            beneficiaryAccountNo = dtIctcBeftn.Rows[rCnt]["BANK_ACCOUNT_NUMBER"].ToString();
                            beneficiaryName = dtIctcBeftn.Rows[rCnt]["BENEFICIARY_NAME"].ToString();
                            bankName = dtIctcBeftn.Rows[rCnt]["BANK_NAME"].ToString();
                            routingNumber = dtIctcBeftn.Rows[rCnt]["BANK_BRANCHCODE"].ToString();
                            branchName = mg.GetBranchNameByRoutingCode(userId, routingNumber);
                            
                            if (!branchName.Equals(""))
                            {
                                beneficiaryAddress = dtIctcBeftn.Rows[rCnt]["BENEFICIARY_ADDRESS"].ToString();
                                senderName = dtIctcBeftn.Rows[rCnt]["REMITTER_NAME"].ToString();
                                senderAddress = dtIctcBeftn.Rows[rCnt]["REMITTER_ADDRESS"].ToString();
                                transferCurrency = "053";
                                receivingAmount = decimal.Round(Convert.ToDecimal(dtIctcBeftn.Rows[rCnt]["PAYING_AMOUNT"].ToString()), 2);
                                paymentDescription = dtIctcBeftn.Rows[rCnt]["PURPOSE_REMIT"].ToString();
                                string paymntResp = "";

                                try
                                {
                                    var nodeBeftnPaymentRequest = remitServiceClient.BEFTNPayment(partyId, exhUserId, passwd, refNo, beneficiaryAccountNo, "SB", beneficiaryName, bankName,
                                        branchName, routingNumber, beneficiaryAddress, senderName, senderAddress, transferCurrency, receivingAmount, paymentDescription);
                                    
                                    //paymntResp = nodeBeftnPaymentRequest.InnerXml;

                                    paymntResp = nodeBeftnPaymentRequest.ToString();
                                    if (!paymntResp.Contains("BEFTNPayment"))
                                    {
                                        paymntResp = "<BEFTNPaymentResponse>" + paymntResp + "</BEFTNPaymentResponse>";
                                    }

                                    xDoc.LoadXml(paymntResp);
                                    //xDoc.LoadXml(nodeBeftnPaymentRequest.ToString());

                                    msgCode = xDoc.GetElementsByTagName("MessageCode");
                                    msgCodeValue = msgCode[0].InnerText;
                                }
                                catch (Exception ex)
                                {
                                    if (IS_INSERT_TO_LOG_TABLE)
                                    { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "BEFTNPayment Error: " + ex); }
                                }

                                if (!msgCodeValue.Equals("") && msgCodeValue.Equals("1020"))    //BEFTN success
                                {
                                    try
                                    {
                                        if (IS_INSERT_TO_LOG_TABLE)
                                        { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "Before ICTC ConfirmTransaction: " + " refNo=" + refNo); }

                                        ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "Y", "");

                                        if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
                                        {
                                            mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "Y", "");
                                            Console.WriteLine("ICTC_Number -> " + refNo + " , BEFTN Txn PAID OK.");

                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "RefNo=" + refNo + ", BEFTN Txn Update at DB Complete.."); }
                                        }
                                        else 
                                        {
                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "ICTC ConfirmTransaction ERROR!!! ," + " refNo=" + refNo); }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (IS_INSERT_TO_LOG_TABLE)
                                        { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "Error: ProcessBEFTNTxn, " + ex.ToString()); }
                                    }
                                }
                                else
                                {
                                    msgVal = xDoc.GetElementsByTagName("Message");
                                    string msgValue = msgVal[0].InnerText;

                                    if (msgCodeValue.Equals("1017")) // Duplicate Reference Number
                                    {
                                        RejectBEFTNDueToDuplicateICNumber(userId, refNo, msgValue);
                                    }

                                    if (IS_INSERT_TO_LOG_TABLE)
                                    { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "ERROR!, BEFTN Fund transfer Failed.. ," + " refNo=" + refNo); }
                                }
                            }
                            else
                            {
                                if (IS_INSERT_TO_LOG_TABLE)
                                { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "ERROR!!! InvalidRoutingNumber, " + " refNo=" + refNo); }

                                RejectBEFTNDueToInvalidRoutingNumber(userId, refNo, routingNumber);
                            }

                        }
                    } //if END
                } //for END
            } //if END
        }

        private static void RejectBEFTNDueToDuplicateICNumber(string userId, string refNo, string msgValue)
        {
            try
            {
                //ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "X", msgValue);

                mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "X", msgValue);
                Console.WriteLine("ICTC_Number -> " + refNo + "  " + msgValue);

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectBEFTNDueToDuplicateICNumber", "RefNo=" + refNo + ", BEFTN Txn Update at DB Complete.."); }
            }
            catch (Exception ex)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectBEFTNDueToDuplicateICNumber", "RefNo=" + refNo + ", RejectBEFTNDueToDuplicateICNumber Error: " + ex); }
            }
        }

        private static void RejectBEFTNDueToInvalidRoutingNumber(string userId, string refNo, string routingNumber)
        {
            try
            {
                ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "X", "Invalid BankBranch Code");
                
                if (IS_INSERT_TO_LOG_TABLE)
                { 
                    mg.InsertAutoFetchLog(userId, "RejectBEFTNDueToInvalid", "RefNo=" + refNo + ", confirmTranResp.Error_Code=" + confirmTranResp.Error_Code+
                    ", " + confirmTranResp.Error_Message + ", " + confirmTranResp.Error_Description);
                }
                
                mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "X", "Invalid BankBranch Code");
                Console.WriteLine("ICTC_Number -> " + refNo + "  Invalid BankBranch Code");

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectBEFTNDueToInvalidRoutingNumber", "RefNo=" + refNo + ", BEFTN Txn Update at DB Complete.."); }
            }
            catch (Exception ex)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "RejectBEFTNDueToInvalidRoutingNumber", "RefNo=" + refNo + ", RejectBEFTNDueToInvalidRoutingNumber Error: " + ex); }
            }
        }

        private static DataTable GetICTCBEFTNAccData(string frmDate1, string frmDate2)
        {
            DataTable remittanceData = new DataTable();
            try
            {
                remittanceData = mg.GetIctcBEFTNRemittanceDetailsByDate(userId, frmDate1, frmDate2);
            }
            catch (Exception exc)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessBEFTNTxn", "ERROR: GetICTCBEFTNAccData, " + exc.ToString()); }
            }
            return remittanceData;
        }

        private static void ProcessOwnAccountCreditTxn()
        {
            string exhUserId, exhAccountNo, beneficiaryAccountNo, beneficiaryName, msgCodeValue, refrnNo;
            string autoIDictc, txnStatus = "", refNo, SenderName, senderPhoneNo = "", senderAddress, senderCountry, bankId, branchId, transferCurrency, msgToBenfcry;
            int partyId;
            string remitPaymentStatus = "";
            XmlDocument xDoc = new XmlDocument();
            XmlNodeList msgCode;

            string frmDate = DateTime.Now.ToString("dd-MMM-yyyy");
            DataTable dtIctcMtbAc = GetICTCOwnAccData(frmDate, frmDate);

            if (IS_INSERT_TO_LOG_TABLE)
            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "Date: " + frmDate + ", ICTCOwnAccData Row Count=" + dtIctcMtbAc.Rows.Count); }

            if (dtIctcMtbAc.Rows.Count > 0)
            {

                Console.WriteLine("dtIctcMtbAc Rows: " + dtIctcMtbAc.Rows.Count + " -->" + DateTime.Now);

                for (int ii = 0; ii < dtIctcMtbAc.Rows.Count; ii++)
                {
                    DataRow drow = dtIctcMtbAc.Rows[ii];
                    txnStatus = drow["TXN_STATUS"].ToString();

                    if (!txnStatus.Equals("") && txnStatus.Equals("RECEIVED"))
                    {
                        refrnNo = drow["ICTC_NUMBER"].ToString();
                        Console.WriteLine("Processing : " + refrnNo + " -->" + DateTime.Now);

                        DataTable dtRemitFundTransferInfo = mg.GetOwnAccountRemitTransferInfo(userId, refrnNo);

                        if (dtRemitFundTransferInfo.Rows.Count > 0)
                        {
                            if (IS_INSERT_TO_LOG_TABLE)
                            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "Error: RefNo=" + refrnNo + ", ALready exists in FundTransfer table, Need to Update ICTC table status"); }

                            remitPaymentStatus = dtRemitFundTransferInfo.Rows[0]["PaymentStatus"].ToString();

                            if (!remitPaymentStatus.Equals("") && remitPaymentStatus.Equals("5"))
                            {
                                autoIDictc = drow["AutoId"].ToString();
                                UpdateOwnAccountRemitAtICTCEndAndTable(refrnNo, autoIDictc);
                            }
                            else 
                            {
                                autoIDictc = drow["AutoId"].ToString();
                                //UpdateFailedOwnAccountRemitAtICTCTable(refrnNo, autoIDictc);

                                if (IS_INSERT_TO_LOG_TABLE)
                                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR!! RefNo=" + refrnNo + ", PaymentStatus=" + remitPaymentStatus); }  
                            }
                        }
                        else
                        {
                            autoIDictc = drow["AutoId"].ToString();
                            DataTable dtIcOwnBank = mg.GetICTCOwnBankRemitData(userId, autoIDictc);

                            partyId = Definitions.AccessCode.Values.ICTCPartyID;
                            passwd = Definitions.AccessCode.Values.ICTCPassword;

                            DataTable dtExchAccInfo = mg.GetExchangeHouseAccountNo(userId, partyId.ToString());
                            exhUserId = dtExchAccInfo.Rows[0]["UserId"].ToString();
                            exhAccountNo = dtExchAccInfo.Rows[0]["AccountNo"].ToString();

                            beneficiaryAccountNo = dtIcOwnBank.Rows[0]["BANK_ACCOUNT_NUMBER"].ToString();
                            beneficiaryName = dtIcOwnBank.Rows[0]["BENEFICIARY_NAME"].ToString();

                            try
                            {
                                refNo = dtIcOwnBank.Rows[0]["ICTC_NUMBER"].ToString();
                                beneficiaryAccountNo = dtIcOwnBank.Rows[0]["BANK_ACCOUNT_NUMBER"].ToString();
                                beneficiaryName = dtIcOwnBank.Rows[0]["BENEFICIARY_NAME"].ToString();
                                if (beneficiaryName.Length > 99)
                                {
                                    beneficiaryName = beneficiaryName.Substring(0, 99);
                                }

                                decimal receivingAmount = decimal.Round(Convert.ToDecimal(dtIcOwnBank.Rows[0]["PAYING_AMOUNT"].ToString()), 2);

                                SenderName = dtIcOwnBank.Rows[0]["REMITTER_NAME"].ToString();
                                if (SenderName.Length > 99)
                                {
                                    SenderName = SenderName.Substring(0, 99);
                                }

                                senderAddress = dtIcOwnBank.Rows[0]["REMITTER_ADDRESS"].ToString();
                                senderCountry = dtIcOwnBank.Rows[0]["REMITTER_NATIONALITY"].ToString();
                                bankId = "001";
                                branchId = ""; 
                                DateTime paymentDate = DateTime.Now;
                                transferCurrency = "053";
                                msgToBenfcry = dtIcOwnBank.Rows[0]["PURPOSE_REMIT"].ToString();

                                //----------------------------------------------------
                                
                                string availBal = "0";
                                try
                                {
                                    CurrentBalanceByAccountNoRequest currBalReq = new CurrentBalanceByAccountNoRequest();
                                    currBalReq.accNo = exhAccountNo;
                                    currBalReq.userName = Definitions.AccessCode.Values.CORE_SERVICE_USERNAME;
                                    currBalReq.password = Definitions.AccessCode.Values.CORE_SERVICE_PASSWORD;

                                    ServicePointManager.Expect100Continue = true;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                           | SecurityProtocolType.Tls11
                                           | SecurityProtocolType.Tls12
                                           | SecurityProtocolType.Ssl3;

                                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;


                                    var resCurrBalance = new CoreMiddlewareService.MTBWebServicePortTypeClient().serCurrentBalance(currBalReq);
                                    string respCode = resCurrBalance.resCode.Trim();
                                    if (respCode.Equals("000"))
                                    {
                                        availBal = resCurrBalance.accInfo.availableBalance;

                                        if (IS_INSERT_TO_LOG_TABLE)
                                        { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refNo + ", ExH Balance=" + availBal); }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (IS_INSERT_TO_LOG_TABLE)
                                    { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR! Balance Check Problem, RefNo=" + refNo); }
                                }

                                decimal exhouseAccountBalance = decimal.Round(Convert.ToDecimal(availBal), 2);
                                if (IS_INSERT_TO_LOG_TABLE)
                                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refNo + ", ExH Balance=" + exhouseAccountBalance); }
                                
                                //-----------------------------------------------------

                                if (exhouseAccountBalance < decimal.Round(Convert.ToDecimal(receivingAmount), 2))
                                {
                                    if (IS_INSERT_TO_LOG_TABLE)
                                    { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR! Balance LOW, RefNo=" + refNo + ", exhAccountBalance=" + exhouseAccountBalance); }
                                }
                                else
                                {
                                    string paymntResp = "";
                                    msgCodeValue = "";
                                    try
                                    {
                                        var nodePaymentRequest = remitServiceClient.Payment("1", partyId, exhUserId, passwd, refNo, beneficiaryAccountNo, beneficiaryName, SenderName,
                                            senderPhoneNo, senderAddress, senderCountry, bankId, branchId, paymentDate, transferCurrency, receivingAmount, "", msgToBenfcry, "");

                                        //paymntResp = nodePaymentRequest.InnerXml;

                                        paymntResp = nodePaymentRequest.ToString();
                                        if (!paymntResp.Contains("PaymentResponse"))
                                        {
                                            paymntResp = "<PaymentResponse>" + paymntResp + "</PaymentResponse>";
                                        }

                                        //xDoc.LoadXml(nodePaymentRequest.ToString());
                                        xDoc.LoadXml(paymntResp);

                                        if (IS_INSERT_TO_LOG_TABLE)
                                        { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refNo + ", PaymentRequest=" + paymntResp); }

                                        msgCode = xDoc.GetElementsByTagName("MessageCode");
                                        msgCodeValue = msgCode[0].InnerText;

                                        Console.WriteLine("RefNo=" + refNo + ", paymntResp : " + paymntResp + " -->" + DateTime.Now);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (IS_INSERT_TO_LOG_TABLE)
                                        { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR! Payment " + ex.ToString()); }
                                    }

                                    if (!msgCodeValue.Equals("") && msgCodeValue.Equals("1009"))    // Fund Transfer Success
                                    {
                                        try
                                        {
                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "Before ICTC ConfirmTransaction: " + " refNo=" + refNo); }

                                            ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "Y", "");

                                            if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
                                            {
                                                mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "Y", "");
                                                Console.WriteLine("ICTC_Number -> " + refNo + " , MTB AccountCredit Txn PAID OK.");

                                                if (IS_INSERT_TO_LOG_TABLE)
                                                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refNo + ", Own Account Txn Update at DB Complete.."); }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "Error: ConfirmAccountCreditPayment, " + ex.ToString()); }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            string msgValue = xDoc.GetElementsByTagName("Message")[0].InnerText;

                                            //ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refNo, "X", msgValue);
                                            //Console.WriteLine("ICTC_Number -> " + refNo + " , Confirmed=" + confirmTranResp.Confirmed + " , Desc=" + confirmTranResp.Description);

                                            mg.UpdateConfirmDownloadAccountCreditTxnTable(refNo, "F", msgValue);

                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR! RefNo=" + refNo + ", Payment Fail :" + msgValue); }

                                            //mg.MarkOwnBankTxnCancelled(userId, autoIDictc, refNo, msgValue);
                                        }
                                        catch (Exception expmt)
                                        {
                                            if (IS_INSERT_TO_LOG_TABLE)
                                            { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR: cancelNBLOwnAccountData, " + expmt.ToString()); }
                                        }
                                    }
                                
                                } //------- balance check ELSE

                            }
                            catch (Exception ex)
                            {
                                if (IS_INSERT_TO_LOG_TABLE)
                                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + dtIcOwnBank.Rows[0]["ICTC_NUMBER"].ToString() + ", AccountNo=" + beneficiaryAccountNo + ", AccountEnquiry ERROR: " + ex.ToString()); }
                            }

                        }//else END

                    } // if RECEIVED END

                }//for END

            } // if row count>0

        }

        private static void UpdateFailedOwnAccountRemitAtICTCTable(string refrnNo, string autoIDictc)
        {
            try
            {
                //ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refrnNo, "X", "Fund transfer failed");
                                
                mg.UpdateConfirmDownloadAccountCreditTxnTable(refrnNo, "X", "Fund transfer failed");
                Console.WriteLine("ICTC_Number -> " + refrnNo + "  Fund transfer failed");

                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refrnNo + ", Own Account Txn Update at DB Complete.."); }                
            }
            catch (Exception ex)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refrnNo + ", ConfirmAccountCreditPayment Error: " + ex); }
            }
        }

        private static void UpdateOwnAccountRemitAtICTCEndAndTable(string refrnNo, string autoIDictc)
        {
            try
            {
                ConfirmTranResponse confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, refrnNo, "Y", "");

                if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
                {
                    mg.UpdateConfirmDownloadAccountCreditTxnTable(refrnNo, "Y", "");
                    Console.WriteLine("ICTC_Number -> " + refrnNo + "  PAID OK.");
                    
                    if (IS_INSERT_TO_LOG_TABLE)
                    { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refrnNo + ", Own Account Txn Update at DB Complete.."); }
                }
            }
            catch (Exception ex)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "RefNo=" + refrnNo + ", ConfirmAccountCreditPayment Error: " + ex); }
            }
        }

        private static DataTable GetICTCOwnAccData(string frmDate1, string frmDate2)
        {
            DataTable remittanceData = new DataTable();
            try
            {
                remittanceData = mg.GetIctcMTBRemittanceDetailsByDate(userId, frmDate1, frmDate2);
            }
            catch (Exception exc)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "ProcessOwnAccountCreditTxn", "ERROR: GetICTCOwnAccData, " + exc.ToString()); }
            }
            return remittanceData;
        }

        private static void DownloadAccountTxn()
        {
            int recordCount = 0;
            int confirmCount = 0;
            string statusMsg = "";
            ConfirmTranResponse confirmTranResp = new ConfirmTranResponse();

            try
            {   
                OutstandingRemittanceResponse acDownloadResponse = ictcclient.DownloadAcTxnResult(Definitions.AccessCode.Values.ICTCSecurityCode);

                if(acDownloadResponse.Result_Flag.Equals("1"))
                {
                    foreach(OutstandingRemits acctxn in acDownloadResponse.outstandingRemittanceList)
                    {
                        try
                        {
                            statusMsg = "";
                            bool isSaved = mg.InsertIntoICDataTable(acctxn, downloadBranch, downloadUser, ref statusMsg);
                            if (isSaved)
                            {
                                recordCount++;
                                if (statusMsg.Equals(""))
                                {
                                    confirmTranResp = ictcclient.ConfirmTransaction(Definitions.AccessCode.Values.ICTCSecurityCode, acctxn.ICTC_Number, "D", "");
                                    if (confirmTranResp.Result_Flag.Equals("1") && confirmTranResp.Confirmed.Equals("true"))
                                    {
                                        confirmCount++;
                                        mg.UpdateConfirmDownloadAccountCreditTxnTable(acctxn.ICTC_Number, "D", "");
                                        Console.WriteLine("ICTC_Number -> " + acctxn.ICTC_Number + "  Downloaded OK.");
                                    }
                                }
                                else
                                {
                                    if (IS_INSERT_TO_LOG_TABLE)
                                    { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "ERROR! INVALID Payment Mode, " + acctxn.ICTC_Number);  
                                        mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "ERROR! Rejecting Invalid Txn, " + acctxn.ICTC_Number); }

                                    RejectBEFTNDueToInvalidRoutingNumber(userId, acctxn.ICTC_Number, "");
                                }
                            }
                            else
                            {
                                if (IS_INSERT_TO_LOG_TABLE)
                                { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "ERROR! SAVING.. InsertIntoICDataTable, " + acctxn.ICTC_Number); }
                            }
                        }
                        catch (Exception excp)
                        {
                            if (IS_INSERT_TO_LOG_TABLE)
                            { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "ERROR! InsertIntoICDataTable " + excp.ToString()); }
                        }
                    }

                    if (recordCount > 0 && confirmCount > 0)
                    {
                        if (IS_INSERT_TO_LOG_TABLE)
                        { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "Download RecordCount=" + recordCount + " , Download Mark=" + confirmCount); }

                        Console.WriteLine("Download RecordCount=" + recordCount + " , Download Mark=" + confirmCount);
                    }
                    else
                    {
                        if (IS_INSERT_TO_LOG_TABLE)
                        { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "confirmTranResp=" + confirmTranResp.ICTC_Number + ", " + confirmTranResp.Description); }
                    }
                }
                else 
                {
                    string errCd = acDownloadResponse.Error_Code;
                    string errMs = acDownloadResponse.Error_Message;
                    if (IS_INSERT_TO_LOG_TABLE)
                    { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "Outstanding Remittance RESPONSE=" + errCd + " , " + errMs); }

                    Console.WriteLine("Outstanding Remittance RESPONSE=" + errCd + " , " + errMs);
                }

            }
            catch (Exception exc)
            {
                if (IS_INSERT_TO_LOG_TABLE)
                { mg.InsertAutoFetchLog(userId, "DownloadAccountCreditTxn", "ERROR! ictcclient Download " + exc.ToString()); }
            }

        } // method END

        
    }
}
