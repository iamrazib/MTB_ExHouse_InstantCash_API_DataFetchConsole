using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICTCDataAutoFetchConsole
{
    class Manager
    {
        public string remittanceConnectionString = Utility.DecryptString(ConfigurationManager.ConnectionStrings["RemittanceDBConnectionString"].ConnectionString.Trim());
        MTBDBManager dbManager = null;



        public void InsertAutoFetchLog(string userId, string methodName, string responseMessage)
        {
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                string query = "INSERT INTO [RemittanceDB].[dbo].[APIDataAutoFetchLog]([UserId],[MethodName],[ResponseMessage]) VALUES('" + userId + "', '" + methodName + "', '" + responseMessage + "')";
                bool status = dbManager.ExcecuteCommand(query);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
        }


        internal bool InsertIntoICDataTable(ICTCServiceClient.OutstandingRemits acctxn, string downloadBranch, string downloadUser, ref string statusMsg)
        {
            SqlConnection openCon = new SqlConnection(remittanceConnectionString);
            SqlCommand cmdSaveAcData = new SqlCommand();

            if (openCon.State.Equals(ConnectionState.Closed))
            {
                openCon.Open();
            }

            cmdSaveAcData.CommandType = CommandType.StoredProcedure;
            cmdSaveAcData.CommandText = "ICTCSpInsertAccountAndCashTxnData";
            cmdSaveAcData.Connection = openCon;

            cmdSaveAcData.Parameters.Add("@Ictc_Number", SqlDbType.VarChar).Value = acctxn.ICTC_Number.Trim();
            cmdSaveAcData.Parameters.Add("@Agent_Ordernumber", SqlDbType.VarChar).Value = acctxn.Agent_OrderNumber == null ? "" : acctxn.Agent_OrderNumber.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Name", SqlDbType.VarChar).Value = acctxn.Remitter_Name == null ? "" : acctxn.Remitter_Name.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Address", SqlDbType.VarChar).Value = acctxn.Remitter_Address == null ? "" : acctxn.Remitter_Address.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Idtype", SqlDbType.VarChar).Value = acctxn.Remitter_IDType == null ? "" : acctxn.Remitter_IDType.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Iddtl", SqlDbType.VarChar).Value = acctxn.Remitter_IDDtl == null ? "" : acctxn.Remitter_IDDtl.Trim();
            cmdSaveAcData.Parameters.Add("@Originating_Country", SqlDbType.VarChar).Value = acctxn.Originating_Country == null ? "" : acctxn.Originating_Country.Trim();
            cmdSaveAcData.Parameters.Add("@Delivery_Mode", SqlDbType.VarChar).Value = acctxn.Delivery_Mode == null ? "" : acctxn.Delivery_Mode.Trim();
            cmdSaveAcData.Parameters.Add("@Paying_Amount", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(acctxn.Paying_Amount), 2);
            cmdSaveAcData.Parameters.Add("@Payingagent_Commshare", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(acctxn.PayingAgent_CommShare), 2);
            cmdSaveAcData.Parameters.Add("@Paying_Currency", SqlDbType.VarChar).Value = acctxn.Paying_Currency == null ? "" : acctxn.Paying_Currency.Trim();
            cmdSaveAcData.Parameters.Add("@Paying_Agent", SqlDbType.VarChar).Value = acctxn.Paying_Agent == null ? "" : acctxn.Paying_Agent.Trim();
            cmdSaveAcData.Parameters.Add("@Paying_Agentname", SqlDbType.VarChar).Value = acctxn.Paying_AgentName == null ? "" : acctxn.Paying_AgentName.Trim();
            cmdSaveAcData.Parameters.Add("@Beneficiary_Name", SqlDbType.VarChar).Value = acctxn.Beneficiary_Name == null ? "" : acctxn.Beneficiary_Name.Trim();
            cmdSaveAcData.Parameters.Add("@Beneficiary_Address", SqlDbType.VarChar).Value = acctxn.Beneficiary_Address == null ? "" : acctxn.Beneficiary_Address.Trim();
            cmdSaveAcData.Parameters.Add("@Beneficiary_City", SqlDbType.VarChar).Value = acctxn.Beneficiary_City == null ? "" : acctxn.Beneficiary_City.Trim();
            cmdSaveAcData.Parameters.Add("@Destination_Country", SqlDbType.VarChar).Value = acctxn.Destination_Country == null ? "" : acctxn.Destination_Country.Trim();
            cmdSaveAcData.Parameters.Add("@Beneficiary_Telno", SqlDbType.VarChar).Value = acctxn.Beneficiary_TelNo == null ? "" : acctxn.Beneficiary_TelNo.Trim();
            cmdSaveAcData.Parameters.Add("@Beneficiary_Mobileno", SqlDbType.VarChar).Value = acctxn.Beneficiary_MobileNo == null ? "" : acctxn.Beneficiary_MobileNo.Trim();
            cmdSaveAcData.Parameters.Add("@Expected_Benefid", SqlDbType.VarChar).Value = acctxn.Expected_BenefID == null ? "" : acctxn.Expected_BenefID.Trim();
            cmdSaveAcData.Parameters.Add("@Bank_Address", SqlDbType.VarChar).Value = acctxn.Bank_Address == null ? "" : acctxn.Bank_Address.Trim();
            cmdSaveAcData.Parameters.Add("@Bank_Account_Number", SqlDbType.VarChar).Value = acctxn.Bank_Account_Number == null ? "" : acctxn.Bank_Account_Number.Trim();
            cmdSaveAcData.Parameters.Add("@Bank_Name", SqlDbType.VarChar).Value = acctxn.Bank_Name == null ? "" : acctxn.Bank_Name.Trim();
            cmdSaveAcData.Parameters.Add("@Purpose_Remit", SqlDbType.VarChar).Value = acctxn.Purpose_Remit == null ? "" : acctxn.Purpose_Remit.Trim();
            cmdSaveAcData.Parameters.Add("@Message_Payeebranch", SqlDbType.VarChar).Value = acctxn.Message_PayeeBranch == null ? "" : acctxn.Message_PayeeBranch.Trim();
            cmdSaveAcData.Parameters.Add("@Bank_Branchcode", SqlDbType.VarChar).Value = acctxn.Bank_BranchCode == null ? "" : acctxn.Bank_BranchCode.Trim();
            cmdSaveAcData.Parameters.Add("@Settlement_Rate", SqlDbType.Float).Value = acctxn.Settlement_Rate == null ? 0 : Math.Round(Convert.ToDouble(acctxn.Settlement_Rate), 2);
            cmdSaveAcData.Parameters.Add("@Prin_Setl_Amount", SqlDbType.Float).Value = acctxn.PrinSettlement_Amount == null ? 0 : Math.Round(Convert.ToDouble(acctxn.PrinSettlement_Amount), 2);
            cmdSaveAcData.Parameters.Add("@Transaction_Sentdate", SqlDbType.VarChar).Value = acctxn.Transaction_SentDate == null ? "" : acctxn.Transaction_SentDate.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Nationality", SqlDbType.VarChar).Value = acctxn.Remitter_Nationality == null ? "" : acctxn.Remitter_Nationality.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Dob", SqlDbType.VarChar).Value = acctxn.Remitter_DOB == null ? "" : acctxn.Remitter_DOB.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_City", SqlDbType.VarChar).Value = acctxn.Remitter_City == null ? "" : acctxn.Remitter_City.Trim();
            cmdSaveAcData.Parameters.Add("@Remitter_Telno", SqlDbType.VarChar).Value = "";
            cmdSaveAcData.Parameters.Add("@Remitter_Mobileno", SqlDbType.VarChar).Value = "";
            cmdSaveAcData.Parameters.Add("@Beneficiary_Nationality", SqlDbType.VarChar).Value = "";
            cmdSaveAcData.Parameters.Add("@Confirm_Download_Txn", SqlDbType.VarChar).Value = "";
            cmdSaveAcData.Parameters.Add("@Txn_Receive_Date", SqlDbType.DateTime).Value = DateTime.Now;
            cmdSaveAcData.Parameters.Add("@Txn_Status", SqlDbType.VarChar).Value = "RECEIVED";

            if (acctxn.Delivery_Mode.ToLower().Contains("account") && acctxn.Delivery_Mode.ToLower().Contains("same") && acctxn.Bank_Name.ToUpper().Contains("MUTUAL"))
            {
                cmdSaveAcData.Parameters.Add("@Payment_Mode", SqlDbType.VarChar).Value = "OWNBANK";
            }
            else if (acctxn.Delivery_Mode.ToLower().Contains("account") && acctxn.Delivery_Mode.ToLower().Contains("other") && !acctxn.Bank_Name.ToUpper().Contains("MUTUAL"))
            {
                cmdSaveAcData.Parameters.Add("@Payment_Mode", SqlDbType.VarChar).Value = "BEFTN";
            }
            else if (acctxn.Delivery_Mode.ToLower().Contains("mobile"))
            {
                cmdSaveAcData.Parameters.Add("@Payment_Mode", SqlDbType.VarChar).Value = "BKASH";
            }
            else if (acctxn.Delivery_Mode.ToLower().Contains("cash"))
            {
                cmdSaveAcData.Parameters.Add("@Payment_Mode", SqlDbType.VarChar).Value = "CASH";
            }
            else
            {
                cmdSaveAcData.Parameters.Add("@Payment_Mode", SqlDbType.VarChar).Value = "INVALID";
                statusMsg = "INVALID";
            }

            cmdSaveAcData.Parameters.Add("@Downloadbranch", SqlDbType.VarChar).Value = downloadBranch;
            cmdSaveAcData.Parameters.Add("@Downloaduser", SqlDbType.VarChar).Value = downloadUser;
            cmdSaveAcData.Parameters.Add("@Clearingdate", SqlDbType.DateTime).Value = DateTime.Now;
            cmdSaveAcData.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = "";

            try
            {
                int k = cmdSaveAcData.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                InsertAutoFetchLog(Definitions.AccessCode.Values.ICTCUserId, "InsertIntoICDataTable", "Error ! InsertIntoICDataTable" + ex.ToString());
                return false;
            }

            return true;
        }


        internal void UpdateConfirmDownloadAccountCreditTxnTable(string ICTC_Number, string confirmFlag, string remarks)
        {
            try
            {
                string query = "";
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                if(confirmFlag.Equals("D"))
                {
                    query = "UPDATE [RemittanceDB].[dbo].[ICTCRequestData] SET [CONFIRM_DOWNLOAD_TXN]='" + confirmFlag + "' WHERE [ICTC_NUMBER]='" + ICTC_Number + "'";
                }
                else if(confirmFlag.Equals("Y"))
                {
                    query = "UPDATE [RemittanceDB].[dbo].[ICTCRequestData] SET [CONFIRM_DOWNLOAD_TXN]='" + confirmFlag + "', [TXN_STATUS]='PAID', [TXN_PAYMENT_DATE]=getdate(), [ClearingDate]=getdate()  WHERE [ICTC_NUMBER]='" + ICTC_Number + "'";
                }                
                else
                {
                    query = "UPDATE [RemittanceDB].[dbo].[ICTCRequestData] SET [CONFIRM_DOWNLOAD_TXN]='" + confirmFlag + "', [TXN_STATUS]='ERROR', [REMARKS]='" + remarks + "'  WHERE [ICTC_NUMBER]='" + ICTC_Number + "'";
                }

                bool status = dbManager.ExcecuteCommand(query);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
        }

        internal DataTable GetIctcMTBRemittanceDetailsByDate(string userId, string fromDate, string toDate)
        {
            DataTable dt = new DataTable();
            string sqlQuery = string.Empty;
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                sqlQuery = "SELECT [AutoId],[ICTC_NUMBER],[REMITTER_NAME],[REMITTER_ADDRESS],[REMITTER_IDTYPE],[REMITTER_IDDTL],[ORIGINATING_COUNTRY],[DELIVERY_MODE],"
                    + " [PAYING_AMOUNT],[PAYING_CURRENCY],[PAYING_AGENT],[PAYING_AGENTNAME],[BENEFICIARY_NAME],[BENEFICIARY_ADDRESS],[BENEFICIARY_CITY],[DESTINATION_COUNTRY],"
                    + " [BENEFICIARY_TELNO],[BENEFICIARY_MOBILENO],[BANK_ADDRESS],[BANK_ACCOUNT_NUMBER],[BANK_NAME],[PURPOSE_REMIT],[MESSAGE_PAYEEBRANCH],[BANK_BRANCHCODE],"
                    + " [TRANSACTION_SENTDATE],[REMITTER_NATIONALITY],[REMITTER_DOB],[REMITTER_CITY],[REMITTER_TELNO],[REMITTER_MOBILENO],[BENEFICIARY_NATIONALITY],"
                    + " [CONFIRM_DOWNLOAD_TXN] ,[TXN_RECEIVE_DATE],[TXN_STATUS],[PAYMENT_MODE],[REMARKS] "
                    + " FROM [RemittanceDB].[dbo].[ICTCRequestData] "
                    + " WHERE [PAYMENT_MODE]='OWNBANK' AND [TXN_STATUS]='RECEIVED'  AND [CONFIRM_DOWNLOAD_TXN]='D' "
                    //+ " AND convert(date,[TXN_RECEIVE_DATE])>='" + fromDate + "' AND  convert(date,[TXN_RECEIVE_DATE])<='" + toDate + "'"
                    + " ORDER BY [AutoId]"; 

                dt = dbManager.GetDataTable(sqlQuery.Trim());
            }
            catch (Exception exception)
            {
                InsertAutoFetchLog(userId, "GetIctcMTBRemittanceDetailsByDate", "Error ! GetIctcMTBRemittanceDetailsByDate fetch Error. " + ", " + exception.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        internal DataTable GetOwnAccountRemitTransferInfo(string userId, string refrnNo)
        {
            DataTable dt = new DataTable();
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();
                string sqlQuery = "select * from [RemittanceDB].[dbo].[FundTransferRequest] where [RefNo]='" + refrnNo.Trim() + "'";
                dt = dbManager.GetDataTable(sqlQuery.Trim());
            }
            catch (Exception ex)
            {
                InsertAutoFetchLog(userId, "GetOwnAccountRemitTransferInfo", "Error ! GetOwnAccountRemitTransferInfo fetch Error." + ex.ToString());
                //throw ex;
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        
        internal DataTable GetICTCOwnBankRemitData(string userId, string autoIDictc)
        {
            DataTable dt = new DataTable();
            string sqlQuery = string.Empty;
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                sqlQuery = "SELECT [AutoId],[ICTC_NUMBER],[REMITTER_NAME],[REMITTER_ADDRESS],[REMITTER_IDTYPE],[REMITTER_IDDTL],[ORIGINATING_COUNTRY],[DELIVERY_MODE],"
                    + " [PAYING_AMOUNT],[PAYING_CURRENCY],[PAYING_AGENT],[PAYING_AGENTNAME],[BENEFICIARY_NAME],[BENEFICIARY_ADDRESS],[BENEFICIARY_CITY],[DESTINATION_COUNTRY],"
                    + " [BENEFICIARY_TELNO],[BENEFICIARY_MOBILENO],[BANK_ADDRESS],[BANK_ACCOUNT_NUMBER],[BANK_NAME],[PURPOSE_REMIT],[MESSAGE_PAYEEBRANCH],[BANK_BRANCHCODE],"
                    + " [TRANSACTION_SENTDATE],[REMITTER_NATIONALITY],[REMITTER_DOB],[REMITTER_CITY],[REMITTER_TELNO],[REMITTER_MOBILENO],[BENEFICIARY_NATIONALITY],"
                    + " [CONFIRM_DOWNLOAD_TXN] ,[TXN_RECEIVE_DATE],[TXN_STATUS],[PAYMENT_MODE],[REMARKS] "
                    + " FROM [RemittanceDB].[dbo].[ICTCRequestData] "
                    + " WHERE [AutoId]='" + autoIDictc + "' AND [PAYMENT_MODE]='OWNBANK' AND [TXN_STATUS]='RECEIVED' ";

                dt = dbManager.GetDataTable(sqlQuery.Trim());
            }
            catch (Exception exception)
            {
                InsertAutoFetchLog(userId, "GetICTCOwnBankRemitData", "Error ! GetICTCOwnBankRemitData fetch Error. " + ", " + exception.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        internal DataTable GetExchangeHouseAccountNo(string userId, string ExchangeHouseCode)
        {
            DataTable dt = new DataTable();
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();
                dt = dbManager.GetDataTable("SELECT * FROM Users WHERE partyid='" + ExchangeHouseCode + "' ");
            }
            catch (Exception ex)
            {
                InsertAutoFetchLog(userId, "GetExchangeHouseAccountNo", "Error ! GetExchangeHouseAccountNo" + ex.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        internal void MarkOwnBankTxnCancelled(string userId, string autoIDictc, string refNo, string msgValue)
        {
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();
                string sqlQuery = "UPDATE [RemittanceDB].[dbo].[ICTCRequestData] SET [TXN_STATUS]='CANCELLED', [remarks]='" + msgValue + "' , [cancel_date]=getdate(), [CONFIRM_DOWNLOAD_TXN]='X' "
                + " WHERE [ICTC_NUMBER]='" + refNo + "' AND [AutoId]=" + Convert.ToInt32(autoIDictc);

                bool status = dbManager.ExcecuteCommand(sqlQuery);
            }
            catch (Exception ex)
            {
                InsertAutoFetchLog(userId, "MarkOwnBankTxnCancelled", "Error ! MarkOwnBankTxnCancelled Error." + ex.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
        }

        internal DataTable GetIctcBEFTNRemittanceDetailsByDate(string userId, string fromDate, string toDate)
        {
            DataTable dt = new DataTable();
            string sqlQuery = string.Empty;
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                sqlQuery = "SELECT [AutoId],[ICTC_NUMBER],[REMITTER_NAME],[REMITTER_ADDRESS],[REMITTER_IDTYPE],[REMITTER_IDDTL],[ORIGINATING_COUNTRY],[DELIVERY_MODE],"
                    + " [PAYING_AMOUNT],[PAYING_CURRENCY],[PAYING_AGENT],[PAYING_AGENTNAME],[BENEFICIARY_NAME],[BENEFICIARY_ADDRESS],[BENEFICIARY_CITY],[DESTINATION_COUNTRY],"
                    + " [BENEFICIARY_TELNO],[BENEFICIARY_MOBILENO],[BANK_ADDRESS],[BANK_ACCOUNT_NUMBER],[BANK_NAME],[PURPOSE_REMIT],[MESSAGE_PAYEEBRANCH],[BANK_BRANCHCODE],"
                    + " [TRANSACTION_SENTDATE],[REMITTER_NATIONALITY],[REMITTER_DOB],[REMITTER_CITY],[REMITTER_TELNO],[REMITTER_MOBILENO],[BENEFICIARY_NATIONALITY],"
                    + " [CONFIRM_DOWNLOAD_TXN] ,[TXN_RECEIVE_DATE],[TXN_STATUS],[PAYMENT_MODE],[REMARKS] "
                    + " FROM [RemittanceDB].[dbo].[ICTCRequestData] "
                    + " WHERE [PAYMENT_MODE]='BEFTN' AND [TXN_STATUS]='RECEIVED'  AND [CONFIRM_DOWNLOAD_TXN]='D' "
                    //+ " AND convert(date,[TXN_RECEIVE_DATE])>='" + fromDate + "' AND  convert(date,[TXN_RECEIVE_DATE])<='" + toDate + "'"
                    //+ " AND ICTC_NUMBER='800111123' "
                    + " ORDER BY [AutoId]";

                dt = dbManager.GetDataTable(sqlQuery.Trim());
            }
            catch (Exception exception)
            {
                InsertAutoFetchLog(userId, "GetIctcBEFTNRemittanceDetailsByDate", "Error ! GetIctcBEFTNRemittanceDetailsByDate fetch Error. " + ", " + exception.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        internal DataTable GetBeftnRemitInfo(string userId, string refNo)
        {
            DataTable dt = new DataTable();
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();
                string sqlQuery = "select * from [RemittanceDB].[dbo].[BEFTNRequest] where [RefNo]='" + refNo.Trim() + "' AND [PartyId]=" + Definitions.AccessCode.Values.ICTCPartyID;
                dt = dbManager.GetDataTable(sqlQuery.Trim());
            }
            catch (Exception ex)
            {
                InsertAutoFetchLog(userId, "GetBeftnRemitInfo", "Error ! GetBeftnRemitInfo fetch Error." + ex.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return dt;
        }

        internal string GetBranchNameByRoutingCode(string userId, string routingNumber)
        {
            DataTable dt = new DataTable();
            string branchName = "";
            string sqlQuery = string.Empty;
            try
            {
                dbManager = new MTBDBManager(MTBDBManager.DatabaseType.SqlServer, remittanceConnectionString);
                dbManager.OpenDatabaseConnection();

                sqlQuery = "SELECT [MTB Sl No],[MTB Code],[Bank Code],[Agent Name],[Branch Name],[City Name],[District],[Routing Number],[Country] "
                    + " FROM [RemittanceDB].[dbo].[BANK_BRANCH] WHERE [Routing Number]='" + routingNumber + "'";
                dt = dbManager.GetDataTable(sqlQuery.Trim());

                branchName = dt.Rows[0]["Branch Name"].ToString();
            }
            catch (Exception exception)
            {
                InsertAutoFetchLog(userId, "GetBranchNameByRoutingCode", "Error ! GetBranchNameByRoutingCode fetch Error. " + ", " + exception.ToString());
            }
            finally
            {
                dbManager.CloseDatabaseConnection();
            }
            return branchName;
        }


        //internal bool InsertIntoICDataTable(ICTCServiceClient.OutstandingRemits acctxn, string downloadBranch, string downloadUser, ref string statusMsg)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
