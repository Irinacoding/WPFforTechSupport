using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using PolicyService_v._1.Properties;
using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PolicyService_v._1
{
    internal class VirtuDB
    {
        private static SqlConnection ConnectionField;

        public static SqlConnection Connection
        {
            get
            {
                if (ConnectionField == null)
                {

                    if (String.IsNullOrEmpty(Settings.Default.ConnectionSetting))
                    {
                        throw new Exception("В настройках приложения не задан ConnectionString");
                    }

                    ConnectionField = new SqlConnection(Settings.Default.ConnectionSetting);
                }

                return ConnectionField;
            }
        }
        ///<summary>UPDATE POLICY SERIAL IN [dbo].[DocumentData]
        ///<param name=></param>
        ///<param name="serial">NEW SERIAL VALUE FROM UI TEXTBOX</param>
        ///</summary>
        public static bool SERIALInDocumentDataUpdating(Guid policyID, string serial)
        {
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"UPDATE DocumentData
                                            SET SERIAL=@serial
                                            WHERE ID=@id";
                    Command.Parameters.AddWithValue("serial", serial);
                    Command.Parameters.AddWithValue("id", policyID);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// RECEIVING OF POLICY ID (procedure auxiliaire for On_PolicyBodyChangeEventHandler)
        /// </summary>
        /// <param name=></param>
        /// <returns></returns>
        public static Guid PolicyIDGetter(string NUMBER, string SerialBefore)
        {            
                Guid policyID = default(Guid);

                if (ConnectionState.Closed == VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Open();
                }
                try
                {
                    using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                    {
                        Command.CommandType = CommandType.Text;
                        Command.CommandText = @"SELECT ID FROM PolicyRegistry  WHERE NUMBAR=@numbar AND SERIAL=@serial";
                        Command.Parameters.AddWithValue("numbar", NUMBER);
                        Command.Parameters.AddWithValue("serial", SerialBefore);
                        SqlDataReader DR = Command.ExecuteReader();
                        while (DR.Read())
                        {
                            if (!DR.IsDBNull(0))
                            {
                                policyID = DR.GetGuid(0);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (ConnectionState.Closed != VirtuDB.Connection.State)
                    {
                        VirtuDB.Connection.Close();
                    }
                }
                return policyID;           
        }

        /// <summary>
        /// CHANGING OF POLICY SERIAL in XML POLICY BODY (procedure auxiliaire for On_PolicyBodyChangeEventHandler)
        /// </summary>
        /// <param name="policyNumber">Policy NUMBER VALUE FROM UI TEXTBOX</param>
        /// <returns></returns>

        public static bool SERIALInPolicyBodyChanging(string NUMBER, string SERIAL,Guid policyID)
        {           
            string text = SERIAL;
            bool result = false;
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);          
            XmlNodeList list = doc.GetElementsByTagName("Serial");
            foreach (XmlNode node in list)
            {
                node.InnerXml = text;
            }
            return result = SavingXmlPolicyChanging(doc, policyID);
        }

        /// <summary>
        ///  CHANGING OF POLICY SERIAL in DocumentData TABLE (procedure auxiliaire for On_SerialChangingEventHandler)
        /// </summary>
        /// <param name="serial"></param>
        /// <returns></returns>
        public static bool SERIALInPolicyRegistryUpdating(Guid policyID, string serial)
        {
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"UPDATE PolicyRegistry
                                            SET SERIAL=@serial
                                            WHERE ID=@id";
                    Command.Parameters.AddWithValue("serial", serial);
                    Command.Parameters.AddWithValue("id", policyID);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return result;
        }

        /// <summary>
        ///  RECEIVING OF VIP ID (procedure auxiliaire for On_PolicyBodyChangeEventHandler)
        /// </summary>
        /// <param name="Name">Insured Name FROM UI TEXTBOX</param>
        /// <returns></returns>
        /// 
        public static Guid VipIDGetter(string Name)
        {
            Guid vipID = default(Guid);
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"SELECT ID FROM RGS_Travel_VIP  WHERE Name=@Name";
                    Command.Parameters.AddWithValue("Name", Name);
                    SqlDataReader DR = Command.ExecuteReader();
                    while (DR.Read())
                    {
                        if (!DR.IsDBNull(0))
                        {
                            vipID = DR.GetGuid(0);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return vipID;
        }

        /// <summary>
        /// SAVING CHANGES OF POLICY BODY TO MS SQL SERVER (main procedure for saving changes in DB for XML policy)
        /// </summary>
        /// <param name="doc">XmlDOCUMENT To Be Saved</param>
        /// <returns></returns>
        public static bool SavingXmlPolicyChanging(XmlDocument changedDoc, Guid policyID)
        {           
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = Connection.CreateCommand())
                {
                    System.Data.SqlTypes.SqlXml sx;
                    using (XmlNodeReader xnr = new XmlNodeReader(changedDoc))
                    {
                        sx = new System.Data.SqlTypes.SqlXml(xnr);
                    }
                    Command.CommandText = @"UPDATE inspolicy SET policy=@xml WHERE ID=@id";
                    Command.Parameters.Add(new SqlParameter(@"xml", SqlDbType.Xml) { Value = sx });
                    Command.Parameters.AddWithValue("id", policyID);

                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
            catch(Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// DELETE VIP PROPERTY FROM POLICY BODY
        /// </summary>
        /// <param name="NUMBER">Policy NUMBER FROM UI TEXTBOX</param>
        /// <param name="Name">Insured Name FROM UI TEXTBOX</param>
        /// <returns></returns>
        public static bool DeleteVipPropertyInPolicyBody(Guid policyID)
        {
            bool resultFromStringValue = false;
            bool result = false;
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);            
            HashSet<string> names = new HashSet<string>();
            names = InsuredListFromPolicyBodyGetter(policyID);
            List<string> vipIdList = new List<string>();        
            foreach (string item in names)
            {
                var VipId = Convert.ToString(VipIDGetter(item));
                vipIdList.Add(VipId);
            }
            XmlNodeList listWithStringValue = doc.GetElementsByTagName("StringValue");
            foreach (string item in vipIdList)
            {
                for (int i = 0; i < listWithStringValue.Count; i++)
                {
                    if (listWithStringValue[i].InnerText == item)
                    {
                        listWithStringValue[i].InnerText = string.Empty;
                        XmlNode outer = listWithStringValue[i].ParentNode;
                        outer.RemoveAll();
                        resultFromStringValue = true;
                    }
                }
            }
            foreach (string name in names)
            {
                DeleteFromRGS_Travel_VIP(name);
            }      
            if (resultFromStringValue) { result = SavingXmlPolicyChanging(doc, policyID); }
            return result;
        }
        /// <summary>
        /// DeleteFromRGS_Travel_VIP
        /// </summary>
        /// <param name="Name"></param>
        private static void DeleteFromRGS_Travel_VIP(string Name)
        {
            bool result = false;

            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"DELETE FROM [dbo].[RGS_Travel_VIP] WHERE Name=@Name";
                    Command.Parameters.AddWithValue("Name", Name);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }            
        }

        /// <summary>
        /// XML POLICY BODY GETTER FROM DB
        /// </summary>
        /// <param name="NUMBER">POLICY NUMBER FROM UI TEXTBOX</param>
        /// <returns></returns>
        public static XmlDocument PolicyBodyGetter(Guid policyID)
        {           
            XmlDocument doc = new XmlDocument();
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"Select policy from inspolicy where ID=@policyID";
                    Command.Parameters.AddWithValue("policyID", policyID);
                    XmlReader reader = Command.ExecuteXmlReader();
                    doc.Load(reader);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return doc;
        }

        /// <summary>
        /// ADD VIP PROPERTY IN CASE OF INSURER and INSURED COHENCIDENCE
        /// </summary>
        /// <param name="NUMBER">POLICY NUMBER FROM UI TEXTBOX</param>
        /// <param name="Name">Insured Name FROM UI TEXTBOX</param>
        /// <returns></returns>
        public static bool AddSingleVipPropertyInPolicyBody(string Name, string BirthDate, Guid policyID)
        {
            bool result = false;
            bool resultFromInsuredPerson = false;
            XmlDocument doc = new XmlDocument();
            var vipNode = VipNodeCreating(Name, BirthDate, policyID);
            doc = PolicyBodyGetter(policyID);
            XmlDocument doc1 = new XmlDocument();            
            XmlNodeList insured = doc.GetElementsByTagName("InsuredPerson");
            foreach (XmlNode rootNode in insured)
            {     
                 doc1.LoadXml(rootNode.ToString());
                XmlNodeList list1 = doc1.GetElementsByTagName("FirstName");
                if (list1[0].InnerText.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    XmlNode temp = doc.ImportNode(vipNode, true);
                    rootNode.AppendChild(temp);
                    resultFromInsuredPerson = SavingXmlPolicyChanging(doc,policyID);
                }
            }
            if (resultFromInsuredPerson) { result = true; }
            return result;
        }
        /// <summary>
        /// AddSingleVipProperty
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="Name"></param>
        /// <param name="BirthDate"></param>
        /// <returns></returns>
        public static bool AddSingleVipProperty(Guid policyID, string Name, string BirthDate)
        {
            bool result = false;
            bool resultFromInsured = false;
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);
            var vipNode = VipNodeCreating(Name, BirthDate, policyID);
            XmlNode outer;
            XmlNode siblingOuter;
            XmlNode nextSiblingOuter;
            XmlNodeList listWithInsured = doc.GetElementsByTagName("InsuredPerson");

            for (int i = 0; i < 1; i++)
            {
                XmlNode temp = doc.ImportNode(vipNode, true);
                outer = listWithInsured[i].FirstChild;
                siblingOuter = outer.NextSibling;
                nextSiblingOuter = siblingOuter.NextSibling;
                nextSiblingOuter.AppendChild(temp);
                resultFromInsured = true;                                                        
            }              
            if (resultFromInsured)
            {
                XmlNodeList listWithInsuredOrPrincipal = doc.GetElementsByTagName("InsuredOrPrincipal");
                for (int i = 0; i < 1; i++)
                {
                    XmlNode temp = doc.ImportNode(vipNode, true);
                    outer = listWithInsuredOrPrincipal[i].FirstChild;
                    if(outer==null)
                    {
                        throw new NullReferenceException("DOM модель не соответствует шаблонной модели.");
                    }
                    siblingOuter = outer.NextSibling;
                    if (siblingOuter == null)
                    {
                        throw new NullReferenceException("DOM модель не соответствует шаблонной модели.");
                    }                   
                    siblingOuter.AppendChild(temp);
                    result = SavingXmlPolicyChanging(doc, policyID);
                }  
            }
            return result;
        }

        /// <summary>
        /// VipNodeCreating
        /// </summary>
        /// <param name="NUMBER"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public static XmlNode VipNodeCreating( string Name, string BirthDate, Guid policyID)
        {
            XmlNode vipNode;
            XmlDocument doc = new XmlDocument();
            bool result = false;
            doc = PolicyBodyGetter(policyID);
            var VipId =VipIDGetter(Name);
            if (VipId==default(Guid))
            {
                result = AddToRGS_Travel_Vip(Name, BirthDate);
                VipId = VipIDGetter(Name);
            }
            else 
            { 
                result = true;
            }
            //element creation
            XmlNode property = doc.CreateNode(XmlNodeType.Element, "Property", "http://tempuri.org/policy.xsd");
            XmlNode typeCd = doc.CreateNode(XmlNodeType.Element, "TypeCd", "http://tempuri.org/policy.xsd");
            XmlNode id = doc.CreateNode(XmlNodeType.Element, "ID", "http://tempuri.org/policy.xsd");
            XmlNode name = doc.CreateNode(XmlNodeType.Element, "Name", "http://tempuri.org/policy.xsd");
            XmlNode stringValue = doc.CreateNode(XmlNodeType.Element, "StringValue", "http://tempuri.org/policy.xsd");

            //add value to element
            id.InnerText = "14CB64D9-38B2-4052-A34E-D4394D8D3B68";
            name.InnerText = "ID в справочнике VIP";
            if (result)
            { 
                stringValue.InnerText = Convert.ToString(VipId);
            }          

            //element constructor
            typeCd.AppendChild(id);
            typeCd.AppendChild(name);
            property.AppendChild(typeCd);
            property.AppendChild(stringValue);
            vipNode = property;
            return vipNode;
        }

        /// <summary>
        /// AddToRGS_Travel_Vip
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="BirthDate"></param>
        /// <returns></returns>
        public static bool AddToRGS_Travel_Vip(string Name, string BirthDate)
        {          
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"DECLARE @ID UNIQUEIDENTIFIER
                                           SET @ID = NEWID()	                                       	 
                                           INSERT INTO [dbo].[RGS_Travel_VIP] 
                                           Values (@ID, @Name,CONVERT(datetime,@BirthDate))";
                    Command.Parameters.AddWithValue("Name", Name);
                    Command.Parameters.AddWithValue("BirthDate", BirthDate);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
                
            catch(Exception)
            {
                
            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return result;
        }
        
       /// <summary>
        /// DELETE PERSON FROM RGS_Travel_VIP
       /// </summary>
       /// <param name="Name">NAME FROM UITextBox</param>
       /// <param name="BirthDate">BIRTHDATE FROM UITextBox</param>
       /// <returns></returns>
        public static bool DeleteFromRGS_Travel_VIP(string Name,  string BirthDate)
        {
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"DELETE FROM [dbo].[RGS_Travel_VIP] WHERE Name=@Name AND  BirthDate=CONVERT(datetime,@BirthDate)";                                         
                    Command.Parameters.AddWithValue("Name", Name);
                    Command.Parameters.AddWithValue("BirthDate", BirthDate);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 0;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return result;
        }
        /// <summary>
        /// FromInsuredListDelete
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="NameToDelete"></param>
        /// <returns></returns>
        public static bool FromInsuredListDelete( Guid policyID,string NameToDelete)
        {   
            bool finalResult=false;
            string value = string.Empty;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {               
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"SELECT InsuredNamesList FROM PolicyRegistry  WHERE ID=@policyID";
                    Command.Parameters.AddWithValue("policyID", policyID);
                    SqlDataReader DR = Command.ExecuteReader();
                    while (DR.Read())
                    {                     
                        if (!DR.IsDBNull(0))
                        {
                            value = DR.GetString(0);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            List<string> insuredList = new List<string>();           
            Char[] delimiter = new char[] { ' ', ';', ',','1' };
            String[] substrings = value.Split(delimiter);
            
            foreach (var substring in substrings)
                insuredList.Add(substring);
            bool deleteResult = insuredList.Remove(NameToDelete);
           
            if(!deleteResult)
            {
                return finalResult;
            }
            else
            {
                string finalString= StringToDeleteConstructor(insuredList);  
                if (ConnectionState.Closed == VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Open();
                }
                try
                {
                    using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                    {
                        Command.CommandType = CommandType.Text;
                        Command.CommandText = @"UPDATE PolicyRegistry SET InsuredNamesList=@finalString  WHERE ID=@policyID";
                        Command.Parameters.AddWithValue("policyID", policyID);
                        Command.Parameters.AddWithValue("finalString", finalString);
                        var nRows = Command.ExecuteNonQuery();
                        finalResult = nRows > 1;
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    if (ConnectionState.Closed != VirtuDB.Connection.State)
                    {
                        VirtuDB.Connection.Close();
                    }
                }
            }
            if (!finalResult)
            {
                return false;
            }
            else
            {
                finalResult = DeleteInsuredPersonFromPolicyBody(policyID, NameToDelete);
            }
            return finalResult;
        }
        /// <summary>
        /// StringToDeleteConstructor
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string StringToDeleteConstructor(List<string> list)
        {
            StringBuilder sb = new StringBuilder(string.Empty);
            foreach (var item in list)
            {
                sb.Append(item);
            }
             string finalSubstring=sb.ToString();
             return finalSubstring;
        }
        /// <summary>
        /// DeleteInsuredPersonFromPolicyBody
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="NameToDelete"></param>
        /// <returns></returns>
        public static bool DeleteInsuredPersonFromPolicyBody( Guid policyID, string NameToDelete)
        {
            bool result=false;
            XmlDocument doc=new XmlDocument();
            doc = PolicyBodyGetter(policyID);
            XmlNode outer;          
            XmlNodeList listWithStringValue;

            listWithStringValue = doc.GetElementsByTagName("FirstName");
            for (int i = 0; i < 1; i++)
            {
                if (listWithStringValue[i].InnerText.Equals(NameToDelete,StringComparison.InvariantCultureIgnoreCase))
                {
                    listWithStringValue[i].InnerText = string.Empty;
                    outer = listWithStringValue[i].ParentNode;
                    outer.RemoveAll();
                    result = true;
                }
                
            }           
            if (result)
            { 
                result = SavingXmlPolicyChanging(doc,policyID);
            }
            return result;
        }
        /// <summary>
        /// BirthDateChanging
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="newBirthDate"></param>
        /// <param name="Name"></param>
        /// <param name="oldBirthDate"></param>
        /// <returns></returns>
        public static bool BirthDateChanging(Guid policyID, string newBirthDate, string Name, string oldBirthDate)
        {
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"UPDATE PolicyRegistry SET BirthDate=@newBirthDate  WHERE ID=@policyID";
                    Command.Parameters.AddWithValue("policyID", policyID);
                    Command.Parameters.AddWithValue("newBirthDate", newBirthDate);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows > 1;
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            if(result)
            {
                result = BirthDateChangingInPolicyBody(policyID, newBirthDate, Name, oldBirthDate); 
            }
            else
            {
                return result;
            }

            return result;
        }
        /// <summary>
        /// BirthDateChangingInPolicyBody
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="newBirthDate"></param>
        /// <param name="Name"></param>
        /// <param name="oldBirthDate"></param>
        /// <returns></returns>
        public static bool BirthDateChangingInPolicyBody(Guid policyID, string newBirthDate, string Name, string oldBirthDate)
        {
            
            bool finalResult=default(Boolean);
            //string birthDateValue = String.Empty;
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);
            XmlNodeList listWithStringValue = doc.GetElementsByTagName("BirthDate");
            finalResult = ValuesInPolicyComparer(listWithStringValue, newBirthDate, Name, oldBirthDate);           
            if (finalResult)
            {
                finalResult = SavingXmlPolicyChanging(doc,policyID);
            }
            return finalResult;
        }
        /// <summary>
        /// ValuesInPolicyComparer
        /// </summary>
        /// <param name="listWithStringValue"></param>
        /// <param name="newBirthDate"></param>
        /// <param name="Name"></param>
        /// <param name="oldBirthDate"></param>
        /// <returns></returns>
        private static bool  ValuesInPolicyComparer( XmlNodeList listWithStringValue,string newBirthDate, string Name, string oldBirthDate)
        {
            bool result = false;
            foreach(XmlElement elem in listWithStringValue)
            {
                if(elem.InnerText.Equals(oldBirthDate, StringComparison.InvariantCultureIgnoreCase))
                {
                    elem.InnerText = newBirthDate;
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        ///  PARENT PRODUCT ID GETTER
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        public static List<ObjectTreeItem> ParentProductIDGetter(Guid ProductID)
        {
            List<ObjectTreeItem> Result = new List<ObjectTreeItem>();           
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = "SELECT * FROM srv_GetObjectTree(@ProductID, 'u')";
                    Command.Parameters.AddWithValue("ProductID", ProductID);                   
                    SqlDataReader DR = Command.ExecuteReader();
                    while (DR.Read())
                    {
                        ObjectTreeItem Item = new ObjectTreeItem();
                        if (!DR.IsDBNull(1))
                        {
                            Item.Id = DR.GetGuid(1);
                        }
                        if (!DR.IsDBNull(0))
                        {
                            Item.Name = DR.GetString(0);
                        }
                        if (!DR.IsDBNull(5))
                        {
                            Item.DisplayName = DR.GetString(5);
                        }

                        Result.Add(Item);
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return Result;
        }
        /// <summary>
        /// PRODUCT ID GETTER
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public static Guid ProductIDGetter(string Number)
        {
            Guid ProductID = default(Guid);

            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"SELECT ProductID FROM PolicyRegistry  WHERE NUMBAR=@Number";
                    Command.Parameters.AddWithValue("Number", Number);
                    SqlDataReader DR = Command.ExecuteReader();
                    while (DR.Read())
                    {
                        if (!DR.IsDBNull(0)) 
                        {
                            ProductID = DR.GetGuid(0);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
            return ProductID;
        }

        /// <summary>
        /// InsuredNameChanging
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
       public static bool InsuredNameChanging(Guid policyID, string oldName, string newName)
        {
            bool result = false;
            if (ConnectionState.Closed == VirtuDB.Connection.State)
            {
                VirtuDB.Connection.Open();
            }
            try
            {
                using (SqlCommand Command = VirtuDB.Connection.CreateCommand())
                {
                    Command.CommandType = CommandType.Text;
                    Command.CommandText = @"UPDATE PolicyRegistry SET InsuredName=@newName  WHERE ID=@policyID";
                    Command.Parameters.AddWithValue("policyID", policyID);
                    Command.Parameters.AddWithValue("newName", newName);
                    var nRows = Command.ExecuteNonQuery();
                    result = nRows >= 1;
                }
            }           
            catch (Exception)
            {

            }
            finally
            {
                if (ConnectionState.Closed != VirtuDB.Connection.State)
                {
                    VirtuDB.Connection.Close();
                }
            }
           
                result = NameChangingInPolicyBody(policyID, oldName, newName);
                                                
            return result;
            
        }
        /// <summary>
       /// NameChangingInPolicyBody
        /// </summary>
        /// <param name="policyID"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static bool NameChangingInPolicyBody( Guid policyID,string oldName, string newName)
        {
            bool finalResult = false;
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);
            bool result=false;

            XmlNodeList insuredNameValues = doc.GetElementsByTagName("FirstName");
            foreach(XmlNode node in insuredNameValues)
            {
                result=ValuesInPolicyComparer(node, oldName, newName);
                if(result)
                {
                    finalResult = SavingXmlPolicyChanging(doc, policyID);
                }
            }                                                      
            return finalResult;
        }
        /// <summary>
        /// ValuesInPolicyComparer
        /// </summary>
        /// <param name="insuredNameValues"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static bool ValuesInPolicyComparer(XmlNode node, string oldName, string newName)
        {
                bool result = false;
                bool comp = node.InnerText.Trim().Equals(oldName.Trim(), StringComparison.InvariantCultureIgnoreCase);
                if (comp)
                {
                   node.InnerText = newName;
                   result = true;

                }
                return result;                    
        }
        /// <summary>
        /// InsuredListFromPolicyBodyGetter
        /// </summary>
        /// <param name="policyID"></param>
        /// <returns></returns>
        public static HashSet<string> InsuredListFromPolicyBodyGetter (Guid policyID)
        {
            XmlDocument doc = new XmlDocument();
            doc = PolicyBodyGetter(policyID);
            XmlNodeList listWithName = doc.GetElementsByTagName("FirstName");
            HashSet<string> names = new HashSet<string>();
            foreach (XmlNode node in listWithName)
            {
                names.Add(node.InnerText);
            }
            return names;
        }     
    }
}