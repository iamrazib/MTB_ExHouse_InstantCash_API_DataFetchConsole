using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ICTCDataAutoFetchConsole
{
    class Utility
    {
        static string _key = "ABCDEFFEDCBAABCDEFFEDCBAABCDEFFEDCBAABCDEFFEDCBA";
        static string _vector = "ABCDEFFEDCBABCDE";

        public static int GetRandomTINForSMSBanking()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            Random random = new Random();
            int intElements = 4;

            for (int i = 0; i < intElements; i++)
            {
                builder.Append(random.Next(1, 9));
            }

            return Int32.Parse(builder.ToString());
        }

        public static string EncryptString(string stringToEncrypt)
        {
            if (stringToEncrypt == null || stringToEncrypt.Length == 0)
            {
                return "";
            }

            TripleDESCryptoServiceProvider _cryptoProvider = new TripleDESCryptoServiceProvider();
            try
            {
                _cryptoProvider.Key = HexToByte(_key);
                _cryptoProvider.IV = HexToByte(_vector);


                byte[] valBytes = Encoding.Unicode.GetBytes(stringToEncrypt);
                ICryptoTransform transform = _cryptoProvider.CreateEncryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);
                cs.Write(valBytes, 0, valBytes.Length);
                cs.FlushFinalBlock();
                byte[] returnBytes = ms.ToArray();
                cs.Close();
                return Convert.ToBase64String(returnBytes);
            }
            catch
            {
                return "";
            }
        }

        public static string DecryptString(string stringToDecrypt)
        {
            if (stringToDecrypt == null || stringToDecrypt.Length == 0)
            {
                return "";
            }

            TripleDESCryptoServiceProvider _cryptoProvider = new TripleDESCryptoServiceProvider();

            try
            {
                _cryptoProvider.Key = HexToByte(_key);
                _cryptoProvider.IV = HexToByte(_vector);

                byte[] valBytes = Convert.FromBase64String(stringToDecrypt);
                ICryptoTransform transform = _cryptoProvider.CreateDecryptor();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write);
                cs.Write(valBytes, 0, valBytes.Length);
                cs.FlushFinalBlock();
                byte[] returnBytes = ms.ToArray();
                cs.Close();
                return Encoding.Unicode.GetString(returnBytes);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array
        /// </summary>
        /// <param name="hexString">hex value</param>
        /// <returns>byte array</returns>
        /// 
        private static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] =
                Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static string base64Encode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }

        public static String Base64Decode(string x)
        {
            try
            {

                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(x);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                return new String(decoded_char);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Decode" + ex.Message);
            }
        }

        public static string Encode_ISO_8859_1(string strData)
        {


            /*
             ISO 8859-1 (aka Latin-1)
            */

            /*                 figure space                   no-break space */
            strData.Replace((char)0x2007, (char)0x00A0);

            /*                 narrow no-break space          no-break space */
            strData.Replace((char)0x202F, (char)0x00A0);

            /*                 zero width no-break space      no-break space */
            strData.Replace((char)0xFEFF, (char)0x00A0);

            /*                 lira sign                      pound sign */
            strData.Replace((char)0x20A4, (char)0x00A3);

            /*!!!              combining diaeresis \w space   diaeresis */
            strData.Replace("\x0020\x0308", "\x00A8");

            /*                 combining diaeresis            diaeresis */
            strData.Replace((char)0x0308, (char)0x00A8);

            /*                 sound recording copyright      copyright sign */
            strData.Replace((char)0x2117, (char)0x00A9);

            /*                 much less-than                 left-pointing double angle quotation mark * */
            strData.Replace((char)0x226A, (char)0x00AB);

            /*                 left double angle bracket      left-pointing double angle quotation mark * */
            strData.Replace((char)0x300A, (char)0x00AB);

            /*                 reversed not sign              not sign */
            strData.Replace((char)0x2310, (char)0x00AC);

            /*                 mongolian todo soft hyphen     soft hyphen */
            strData.Replace((char)0x1806, (char)0x00AD);

            /*                 modifier letter macron         macron */
            strData.Replace((char)0x02C9, (char)0x00AF);

            /*                 combining macron               macron */
            strData.Replace((char)0x0304, (char)0x00AF);

            /*                 combining overline             macron */
            strData.Replace((char)0x0305, (char)0x00AF);

            /*                 ring above                     degree sign */
            strData.Replace((char)0x02DA, (char)0x00B0);

            /*                 combining ring above           degree sign */
            strData.Replace((char)0x030A, (char)0x00B0);

            /*                 superscript zero               degree sign */
            strData.Replace((char)0x2070, (char)0x00B0);

            /*                 ring operator                  degree sign */
            strData.Replace((char)0x2218, (char)0x00B0);

            /*                 minus-or-plus sign             plus-minus sign */
            strData.Replace((char)0x2213, (char)0x00B1);

            /*                 superscript one                superscript two */
            strData.Replace((char)0x00B9, (char)0x00B2);

            /*                 superscript one                superscript three */
            strData.Replace((char)0x00B9, (char)0x00B3);

            /*                 modifier letter prime          acute accent */
            strData.Replace((char)0x02B9, (char)0x00B4);

            /*                 modifier letter acute accent   acute accent */
            strData.Replace((char)0x02CA, (char)0x00B4);

            /*                 combining acute accent         acute accent */
            strData.Replace((char)0x0301, (char)0x00B4);

            /*                 prime                          acute accent */
            strData.Replace((char)0x2032, (char)0x00B4);

            /*                 reversed pilcrow sign          pilcrow sign */
            strData.Replace((char)0x204B, (char)0x00B6);

            /*                 curved stem paragraph sign ornament pilcrow sign */
            strData.Replace((char)0x2761, (char)0x00B6);

            /*                 bullet                         middle dot */
            strData.Replace((char)0x2022, (char)0x00B7);

            /*                 one dot leader                 middle dot */
            strData.Replace((char)0x2024, (char)0x00B7);

            /*                 hyphenation point              middle dot */
            strData.Replace((char)0x2027, (char)0x00B7);

            /*                 bullet operator                middle dot */
            strData.Replace((char)0x2219, (char)0x00B7);

            /*                 dot operator                   middle dot */
            strData.Replace((char)0x22C5, (char)0x00B7);

            /*                 katakana middle dot            middle dot */
            strData.Replace((char)0x30FB, (char)0x00B7);

            /*                 combining cedilla              cedilla */
            strData.Replace((char)0x0327, (char)0x00B8);

            /*                 much greater-than              right-pointing double angle quotation mark * */
            strData.Replace((char)0x226B, (char)0x00BB);

            /*                 right double angle bracket     right-pointing double angle quotation mark * */
            strData.Replace((char)0x300B, (char)0x00BB);

            /*!!!              'A' + grave				      latin capital letter a with grave */
            strData.Replace("\x0041\x0300", "\x00C0");

            /*!!!              'A' + acute			          latin capital letter a with acute */
            strData.Replace("\x0041\x0301", "\x00C1");

            /*!!!              'A' + circumflex		          latin capital letter a with circumflex */
            strData.Replace("\x0041\x0302", "\x00C2");

            /*!!!              'A' + tilde			          latin capital letter a with tilde */
            strData.Replace("\x0041\x0303", "\x00C3");

            /*!!!              'A' + diaeresis		          latin capital letter a with diaeresis */
            strData.Replace("\x0041\x0308", "\x00C4");

            /*!!!              'A' + ring			          latin capital letter a with ring above */
            strData.Replace("\x0041\x030A", "\x00C5");

            /*                 angstrom sign                  latin capital letter a with ring above */
            strData.Replace((char)0x212B, (char)0x00C5);

            /*!!!              'C' + cedilla                  latin capital letter c with cedilla */
            strData.Replace("\x0043\x0327", "\x00C7");

            /*!!!              'E' + grave                  latin capital letter e with grave */
            strData.Replace("\x0045\x0300", "\x00C8");

            /*!!!              'E' + acute                    latin capital letter e with acute */
            strData.Replace("\x0045\x0301", "\x00C9");

            /*!!!              'E' + circumflex               latin capital letter e with circumflex */
            strData.Replace("\x0045\x0302", "\x00CA");

            /*!!!              'E' + diaeresis                latin capital letter e with diaeresis */
            strData.Replace("\x0045\x0308", "\x00CB");

            /*!!!              'I' + grave                    latin capital letter i with grave */
            strData.Replace("\x0049\x0300", "\x00CC");

            /*!!!              'I' + acute                    latin capital letter i with acute */
            strData.Replace("\x0049\x0301", "\x00CD");

            /*!!!              'I' + circumflex               latin capital letter i with circumflex */
            strData.Replace("\x0049\x0302", "\x00CE");

            /*!!!              'I' + diaeresis                latin capital letter i with diaeresis */
            strData.Replace("\x0049\x0308", "\x00CF");

            /*                 latin capital letter d with stroke latin capital letter eth (icelandic) */
            strData.Replace((char)0x0110, (char)0x00D0);

            /*                 latin capital letter african d latin capital letter eth (icelandic) */
            strData.Replace((char)0x0189, (char)0x00D0);

            /*!!!              'N' + tilde                    latin capital letter n with tidle */
            strData.Replace("\x004E\x0303", "\x00D1");

            /*!!!              'O' + grave                    latin capital letter o with grave */
            strData.Replace("\x004F\x0300", "\x00D2");

            /*!!!              'O' + acute                    latin capital letter o with acute */
            strData.Replace("\x004F\x0301", "\x00D3");

            /*!!!              'O' + circumflex               latin capital letter o with circumflex */
            strData.Replace("\x004F\x0302", "\x00D4");

            /*!!!              'O' + tilde                    latin capital letter o with diaeresis */
            strData.Replace("\x004F\x0303", "\x00D5");

            /*!!!              'O' + diaeresis                latin capital letter o with diaeresis */
            strData.Replace("\x004F\x0308", "\x00D6");

            /*                 empty set                      latin capital letter o with stroke */
            strData.Replace((char)0x2205, (char)0x00D8);

            /*!!!              'U' + grave                    latin capital letter u with grave */
            strData.Replace("\x0055\x0300", "\x00D9");

            /*!!!              'U' + acute                    latin capital letter u with acute */
            strData.Replace("\x0055\x0301", "\x00DA");

            /*!!!              'U' + circumflex               latin capital letter u with circumflex */
            strData.Replace("\x0055\x0302", "\x00DB");

            /*!!!              'U' + diaeresis                latin capital letter u with diaeresis */
            strData.Replace("\x0055\x0308", "\x00DC");

            /*!!!              'Y' + acute                    latin capital letter y with acute */
            strData.Replace("\x0059\x0301", "\x00DD");

            /*                 greek small letter beta        latin small letter sharp s (german) */
            strData.Replace((char)0x03B2, (char)0x00DF);

            /*!!!              'a' + grave				      latin small letter a with grave */
            strData.Replace("\x0061\x0300", "\x00E0");

            /*!!!              'a' + acute			          latin small letter a with acute */
            strData.Replace("\x0061\x0301", "\x00E1");

            /*!!!              'a' + circumflex		          latin small letter a with circumflex */
            strData.Replace("\x0061\x0302", "\x00E2");

            /*!!!              'a' + tilde			          latin small letter a with tilde */
            strData.Replace("\x0061\x0303", "\x00E3");

            /*!!!              'a' + diaeresis		          latin small letter a with diaeresis */
            strData.Replace("\x0061\x0308", "\x00E4");

            /*!!!              'a' + ring			          latin small letter a with ring above */
            strData.Replace("\x0061\x030A", "\x00E5");

            /*!!!              'c' + cedilla                  latin small letter c with cedilla */
            strData.Replace("\x0063\x0327", "\x00E7");

            /*!!!              'e' + grave                    latin small letter e with grave */
            strData.Replace("\x0065\x0300", "\x00E8");

            /*!!!              'e' + acute                    latin small letter e with acute */
            strData.Replace("\x0065\x0301", "\x00E9");

            /*!!!              'e' + circumflex               latin small letter e with circumflex */
            strData.Replace("\x0065\x0302", "\x00EA");

            /*!!!              'e' + diaeresis                latin small letter e with diaeresis */
            strData.Replace("\x0065\x0308", "\x00EB");

            /*!!!              'i' + grave                    latin small letter i with grave */
            strData.Replace("\x0069\x0300", "\x00EC");

            /*!!!              'i' + acute                    latin small letter i with acute */
            strData.Replace("\x0069\x0301", "\x00ED");

            /*!!!              'i' + circumflex               latin small letter i with circumflex */
            strData.Replace("\x0069\x0302", "\x00EE");

            /*!!!              'i' + diaeresis                latin small letter i with diaeresis */
            strData.Replace("\x0069\x0308", "\x00EF");

            /*!!!              'n' + tilde                    latin small letter n with tidle */
            strData.Replace("\x006E\x0303", "\x00F1");

            /*!!!              'o' + grave                    latin small letter o with grave */
            strData.Replace("\x006F\x0300", "\x00F2");

            /*!!!              'o' + acute                    latin small letter o with acute */
            strData.Replace("\x006F\x0301", "\x00F3");

            /*!!!              'o' + circumflex               latin small letter o with circumflex */
            strData.Replace("\x006F\x0302", "\x00F4");

            /*!!!              'o' + tilde                    latin small letter o with diaeresis */
            strData.Replace("\x006F\x0303", "\x00F5");

            /*!!!              'o' + diaeresis                latin small letter o with diaeresis */
            strData.Replace("\x006F\x0308", "\x00F6");

            /*!!!              'u' + grave                    latin small letter u with grave */
            strData.Replace("\x0075\x0300", "\x00F9");

            /*!!!              'u' + acute                    latin capital letter u with acute */
            strData.Replace("\x0075\x0301", "\x00FA");

            /*!!!              'u' + circumflex               latin capital letter u with circumflex */
            strData.Replace("\x0075\x0302", "\x00FB");

            /*!!!              'u' + diaeresis                latin capital letter u with diaeresis */
            strData.Replace("\x0075\x0308", "\x00FC");

            /*!!!              'y' + acute                    latin capital letter y with acute */
            strData.Replace("\x0079\x0301", "\x00FD");

            /*!!!              'y' + diaeresis                0latin capital letter y with diaeresis */
            strData.Replace("\x0079\x0308", "\x00FD");

            /*                 latin small ligature oe        latin small letter ae (ash) * */
            strData.Replace((char)0x0153, (char)0x00E6);

            /*                 cyrillic small ligature a ie   latin small letter ae (ash) * */
            strData.Replace((char)0x04D5, (char)0x00E6);

            /*                 greek small letter delta       latin small letter eth (icelandic) */
            strData.Replace((char)0x03B4, (char)0x00F0);

            /*                 partial differential           latin small letter eth (icelandic) */
            strData.Replace((char)0x2202, (char)0x00F0);

            /*                 runic letter thurisaz thurs thorn latin small letter thorn (icelandic) */
            strData.Replace((char)0x16A6, (char)0x00FE);

            /*                 latin capital letter y with diaeresis latin small letter y with diaeresis */
            strData.Replace((char)0x0178, (char)0x00FF);



            return strData;
        }

        public static string Encode_ASCII(string strData)
        {
            /*
             ASCII
            */

            /*                 zero width space               space */
            strData.Replace((char)0x200B, (char)0x0020);

            /*                 ideographic space              space */
            strData.Replace((char)0x3000, (char)0x0020);

            /*                 zero width no-break space      space */
            strData.Replace((char)0xFEFF, (char)0x0020);

            /*                 latin letter retroflex click   exclamation mark */
            strData.Replace((char)0x01C3, (char)0x0021);

            /*                 double exclamation mark        exclamation mark */
            strData.Replace((char)0x203C, (char)0x0021);

            /*                 interrobang                    exclamation mark */
            strData.Replace((char)0x203D, (char)0x0021);

            /*                 heavy exclamation mark ornament exclamation mark */
            strData.Replace((char)0x2762, (char)0x0021);

            /*                 modifier letter double prime   quotation mark */
            strData.Replace((char)0x02BA, (char)0x0022);

            /*!!!              open quotation mark			  quotation mark */
            strData.Replace((char)0x201C, (char)0x0022);

            /*!!!              close quotation mark			  quotation mark */
            strData.Replace((char)0x201D, (char)0x0022);

            /*                 combining double acute accent  quotation mark */
            strData.Replace((char)0x030B, (char)0x0022);

            /*                 combining double vertical line above quotation mark */
            strData.Replace((char)0x030E, (char)0x0022);

            /*                 double prime                   quotation mark */
            strData.Replace((char)0x2033, (char)0x0022);

            /*                 ditto mark                     quotation mark */
            strData.Replace((char)0x3003, (char)0x0022);

            /*                 arabic percent sign            percent sign */
            strData.Replace((char)0x066A, (char)0x0025);

            /*                 per mille sign                 percent sign */
            strData.Replace((char)0x2030, (char)0x0025);

            /*                 per ten thousand sign          percent sign */
            strData.Replace((char)0x2031, (char)0x0025);

            /*!!!              open apostrophe                apostrophe */
            strData.Replace((char)0x2018, (char)0x0027);

            /*!!!              close apostrophe               apostrophe */
            strData.Replace((char)0x2019, (char)0x0027);

            /*                 modifier letter prime          apostrophe */
            strData.Replace((char)0x02B9, (char)0x0027);

            /*                 modifier letter apostrophe     apostrophe */
            strData.Replace((char)0x02BC, (char)0x0027);

            /*                 modifier letter vertical line  apostrophe */
            strData.Replace((char)0x02C8, (char)0x0027);

            /*                 combining acute accent         apostrophe */
            strData.Replace((char)0x0301, (char)0x0027);

            /*                 prime                          apostrophe */
            strData.Replace((char)0x2032, (char)0x0027);

            /*                 arabic five pointed star       asterisk */
            strData.Replace((char)0x066D, (char)0x002A);

            /*                 asterisk operator              asterisk */
            strData.Replace((char)0x2217, (char)0x002A);

            /*                 heavy asterisk                 asterisk */
            strData.Replace((char)0x2731, (char)0x002A);

            /*                 arabic comma                   comma */
            strData.Replace((char)0x060C, (char)0x002C);

            /*                 single low-9 quotation mark    comma */
            strData.Replace((char)0x201A, (char)0x002C);

            /*                 ideographic comma              comma */
            strData.Replace((char)0x3001, (char)0x002C);

            /*                 hyphen                         hyphen-minus */
            strData.Replace((char)0x2010, (char)0x002D);

            /*                 non-breaking hyphen            hyphen-minus */
            strData.Replace((char)0x2011, (char)0x002D);

            /*                 figure dash                    hyphen-minus */
            strData.Replace((char)0x2012, (char)0x002D);

            /*                 en dash                        hyphen-minus */
            strData.Replace((char)0x2013, (char)0x002D);

            /*                 minus sign                     hyphen-minus */
            strData.Replace((char)0x2212, (char)0x002D);

            /*                 arabic full stop               full stop */
            strData.Replace((char)0x06D4, (char)0x002E);

            /*                 ideographic full stop          full stop */
            strData.Replace((char)0x3002, (char)0x002E);

            /*                 latin letter dental click      solidus */
            strData.Replace((char)0x01C0, (char)0x002F);

            /*                 combining long solidus overlay solidus */
            strData.Replace((char)0x0338, (char)0x002F);

            /*                 fraction slash                 solidus */
            strData.Replace((char)0x2044, (char)0x002F);

            /*                 division slash                 solidus */
            strData.Replace((char)0x2215, (char)0x002F);

            /*                 armenian full stop             colon */
            strData.Replace((char)0x0589, (char)0x003A);

            /*                 ratio                          colon */
            strData.Replace((char)0x2236, (char)0x003A);

            /*                 greek question mark            semicolon */
            strData.Replace((char)0x037E, (char)0x003B);

            /*                 arabic semicolon               semicolon */
            strData.Replace((char)0x061B, (char)0x003B);

            /*                 single left-pointing angle quotation mark less-than sign */
            strData.Replace((char)0x2039, (char)0x003C);

            /*                 left-pointing angle bracket    less-than sign */
            strData.Replace((char)0x2329, (char)0x003C);

            /*                 left angle bracket             less-than sign */
            strData.Replace((char)0x3008, (char)0x003C);

            /*                 not equal to                   equals sign */
            strData.Replace((char)0x2260, (char)0x003D);

            /*                 identical to                   equals sign */
            strData.Replace((char)0x2261, (char)0x003D);

            /*                 single right-pointing angle quotation mark greater-than sign */
            strData.Replace((char)0x203A, (char)0x003E);

            /*                 right-pointing angle bracket   greater-than sign */
            strData.Replace((char)0x232A, (char)0x003E);

            /*                 right angle bracket            greater-than sign */
            strData.Replace((char)0x3009, (char)0x003E);

            /*                 greek question mark            question mark */
            strData.Replace((char)0x037E, (char)0x003F);

            /*                 arabic question mark           question mark */
            strData.Replace((char)0x061F, (char)0x003F);

            /*                 interrobang                    question mark */
            strData.Replace((char)0x203D, (char)0x003F);

            /*                 question exclamation mark      question mark */
            strData.Replace((char)0x2048, (char)0x003F);

            /*                 exclamation question mark      question mark */
            strData.Replace((char)0x2049, (char)0x003F);

            /*                 script capital b               latin capital letter b */
            strData.Replace((char)0x212C, (char)0x0042);

            /*                 double-struck capital c        latin capital letter c */
            strData.Replace((char)0x2102, (char)0x0043);

            /*                 black-letter capital c         latin capital letter c */
            strData.Replace((char)0x212D, (char)0x0043);

            /*                 euler constant                 latin capital letter e */
            strData.Replace((char)0x2107, (char)0x0045);

            /*                 script capital e               latin capital letter e */
            strData.Replace((char)0x2130, (char)0x0045);

            /*                 script capital f               latin capital letter f */
            strData.Replace((char)0x2131, (char)0x0046);

            /*                 turned capital f               latin capital letter f */
            strData.Replace((char)0x2132, (char)0x0046);

            /*                 script capital h               latin capital letter h */
            strData.Replace((char)0x210B, (char)0x0048);

            /*                 black-letter capital h         latin capital letter h */
            strData.Replace((char)0x210C, (char)0x0048);

            /*                 double-struck capital h        latin capital letter h */
            strData.Replace((char)0x210D, (char)0x0048);

            /*                 latin capital letter i with dot above latin capital letter i */
            strData.Replace((char)0x0130, (char)0x0049);

            /*                 cyrillic capital letter byelorussian-ukrainian i latin capital letter i */
            strData.Replace((char)0x0406, (char)0x0049);

            /*                 cyrillic letter palochka       latin capital letter i */
            strData.Replace((char)0x04C0, (char)0x0049);

            /*                 script capital i               latin capital letter i */
            strData.Replace((char)0x2110, (char)0x0049);

            /*                 black-letter capital i         latin capital letter i */
            strData.Replace((char)0x2111, (char)0x0049);

            /*                 roman numeral one              latin capital letter i */
            strData.Replace((char)0x2160, (char)0x0049);

            /*                 kelvin sign                    latin capital letter k */
            strData.Replace((char)0x212A, (char)0x004B);

            /*                 script capital l               latin capital letter l */
            strData.Replace((char)0x2112, (char)0x004C);

            /*                 script capital m               latin capital letter m */
            strData.Replace((char)0x2133, (char)0x004D);

            /*                 double-struck capital n        latin capital letter n */
            strData.Replace((char)0x2115, (char)0x004E);

            /*                 double-struck capital p        latin capital letter p */
            strData.Replace((char)0x2119, (char)0x0050);

            /*                 double-struck capital q        latin capital letter q */
            strData.Replace((char)0x211A, (char)0x0051);

            /*                 script capital r               latin capital letter r */
            strData.Replace((char)0x211B, (char)0x0052);

            /*                 black-letter capital r         latin capital letter r */
            strData.Replace((char)0x211C, (char)0x0052);

            /*                 double-struck capital r        latin capital letter r */
            strData.Replace((char)0x211D, (char)0x0052);

            /*                 double-struck capital z        latin capital letter z */
            strData.Replace((char)0x2124, (char)0x005A);

            /*                 black-letter capital z         latin capital letter z */
            strData.Replace((char)0x2128, (char)0x005A);

            /*                 set minus                      reverse solidus */
            strData.Replace((char)0x2216, (char)0x005C);

            /*                 modifier letter up arrowhead   circumflex accent */
            strData.Replace((char)0x02C4, (char)0x005E);

            /*                 modifier letter circumflex accent circumflex accent */
            strData.Replace((char)0x02C6, (char)0x005E);

            /*                 combining circumflex accent    circumflex accent */
            strData.Replace((char)0x0302, (char)0x005E);

            /*                 up arrowhead                   circumflex accent */
            strData.Replace((char)0x2303, (char)0x005E);

            /*                 modifier letter low macron     low line */
            strData.Replace((char)0x02CD, (char)0x005F);

            /*                 combining macron below         low line */
            strData.Replace((char)0x0331, (char)0x005F);

            /*                 combining low line             low line */
            strData.Replace((char)0x0332, (char)0x005F);

            /*                 double low line                low line */
            strData.Replace((char)0x2017, (char)0x005F);

            /*                 modifier letter grave accent   grave accent */
            strData.Replace((char)0x02CB, (char)0x0060);

            /*                 combining grave accent         grave accent */
            strData.Replace((char)0x0300, (char)0x0060);

            /*                 reversed prime                 grave accent */
            strData.Replace((char)0x2035, (char)0x0060);

            /*                 estimated symbol               latin small letter e */
            strData.Replace((char)0x212E, (char)0x0065);

            /*                 script small e                 latin small letter e */
            strData.Replace((char)0x212F, (char)0x0065);

            /*                 latin small letter script g    latin small letter g */
            strData.Replace((char)0x0261, (char)0x0067);

            /*                 script small g                 latin small letter g */
            strData.Replace((char)0x210A, (char)0x0067);

            /*                 cyrillic small letter shha     latin small letter h */
            strData.Replace((char)0x04BB, (char)0x0068);

            /*                 planck constant                latin small letter h */
            strData.Replace((char)0x210E, (char)0x0068);

            /*                 latin small letter dotless i   latin small letter i */
            strData.Replace((char)0x0131, (char)0x0069);

            /*                 script small l                 latin small letter l */
            strData.Replace((char)0x2113, (char)0x006C);

            /*                 superscript latin small letter n latin small letter n */
            strData.Replace((char)0x207F, (char)0x006E);

            /*                 script small o                 latin small letter o */
            strData.Replace((char)0x2134, (char)0x006F);

            /*                 latin small letter z with stroke latin small letter z */
            strData.Replace((char)0x01B6, (char)0x007A);

            /*                 latin letter dental click      vertical line */
            strData.Replace((char)0x01C0, (char)0x007C);

            /*                 divides                        vertical line */
            strData.Replace((char)0x2223, (char)0x007C);

            /*                 light vertical bar             vertical line */
            strData.Replace((char)0x2758, (char)0x007C);

            /*                 small tilde                    tilde */
            strData.Replace((char)0x02DC, (char)0x007E);

            /*                 combining tilde                tilde */
            strData.Replace((char)0x0303, (char)0x007E);

            /*                 tilde operator                 tilde */
            strData.Replace((char)0x223C, (char)0x007E);

            /*                 fullwidth tilde                tilde */
            strData.Replace((char)0xFF5E, (char)0x007E);


            return strData;
        }

        //public static string GetIPAddress()
        //{
        //    //return System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].GetIPProperties().UnicastAddresses[0].Address.ToString();

        //    System.Web.HttpContext context = System.Web.HttpContext.Current;
        //    string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        //    if (!string.IsNullOrEmpty(ipAddress))
        //    {
        //        string[] addresses = ipAddress.Split(',');
        //        if (addresses.Length != 0)
        //        {
        //            return addresses[0];
        //        }
        //    }

        //    return context.Request.ServerVariables["REMOTE_ADDR"];
        //}

        #region Encrypt Decrypt 256

        /// //////////////////////EDIT : 1 (START)//////////////////////////////////

        //public static string Encrypt256(string textToEncrypt)
        //{
        //    try
        //    {
        //        IOSCyptor.IOSCyptor obj = new IOSCyptor.IOSCyptor();

        //        return obj.EncPlainString(textToEncrypt);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public static string Decrypt256(string encryptedText)
        //{
        //    try
        //    {
        //        IOSCyptor.IOSCyptor obj = new IOSCyptor.IOSCyptor();

        //        return obj.DecBase64String(encryptedText);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
       // }

        /// //////////////////////EDIT : 1 (START)//////////////////////////////////

        #endregion
    }
}
