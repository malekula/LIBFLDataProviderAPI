﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.Text.RegularExpressions;



[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]


// Чтобы разрешить вызывать веб-службу из сценария с помощью ASP.NET AJAX, раскомментируйте следующую строку. 
// [System.Web.Script.Services.ScriptService]
public class Service : System.Web.Services.WebService
{

    public Service () {

        //Раскомментируйте следующую строку в случае использования сконструированных компонентов 
        //InitializeComponent(); 
    }

    


    //[WebMethod]
    public DataSet GetReaderInfoDataSet(int NumberReader)
    {
        SqlDataAdapter da = new SqlDataAdapter();
        da.SelectCommand = new SqlCommand();
        da.SelectCommand.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReadersConnection_OnlyRead"].ConnectionString);
        da.SelectCommand.Parameters.AddWithValue("NumberReader", NumberReader);
        da.SelectCommand.CommandText = "select FamilyName, Name, FatherName, DateBirth from Readers..Main where NumberReader = @NumberReader";
        DataSet ds = new DataSet();
        int cnt = da.Fill(ds);
        if (cnt == 0) throw new Exception("Читатель не найден!");
        return ds;
    }

    [WebMethod(Description = "Возвращает информацию о пользователе. Если пользователь не найден или входной параметр имеет неправильный формат, генерируется исключение.")]
    public ReaderInfo GetReaderInfo(int NumberReader)
    {
        SqlDataAdapter da = new SqlDataAdapter();
        da.SelectCommand = new SqlCommand();
        da.SelectCommand.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReadersConnection_OnlyRead"].ConnectionString);
        da.SelectCommand.Parameters.AddWithValue("NumberReader", NumberReader);
        da.SelectCommand.CommandText = "select FamilyName, Name, FatherName, DateBirth from Readers..Main where NumberReader = @NumberReader";
        DataSet ds = new DataSet();
        int cnt = da.Fill(ds);
        if (cnt == 0) throw new Exception("Читатель не найден!");
        ReaderInfo ri = new ReaderInfo();
        ri.FamilyName = ds.Tables[0].Rows[0]["FamilyName"].ToString();
        ri.Name = ds.Tables[0].Rows[0]["Name"].ToString();
        ri.FatherName = ds.Tables[0].Rows[0]["FatherName"].ToString();
        ri.DateBirth = (DateTime)ds.Tables[0].Rows[0]["DateBirth"];
        ri.IsRemoteReader = (bool)ds.Tables[0].Rows[0]["TypeReader"];
        return ri;
    }




    [WebMethod(Description = "Возвращает true при успешной авторизации. Во всех остальных случаях генерируется исключение. Если пользователь не найден или входной параметр имеет неправильный формат, генерируется исключение. Возможные исключения: "+
                             " \n1. \"Читатель не найден.\" Означает, что читатель не найден в базе ни по номеру читательского билета, ни по номеру социальной карты, ни по email." +
                             " \n2. \"Таких Email найдено больше одного! Введите номер читателя!\"  В базе имеет 250 повторяющихся email. Введен email из этого списка. "+
                             " Невозможно идентифицировать. В этом случае читателя придётся попросить указать номер читательского билета в качестве логина." +
                             " \n3. \"Неверный пароль.\" Означает, что читатель найден, но введён неверный пароль.")]

    public bool Authorize(string login, string password)
    {
        string CommandText = "";
        int NumberReader = 0;
        bool Check = false;
        if (login.Length == 6 && int.TryParse(login, out NumberReader))
        {
            CommandText = "select * from Readers..Main where NumberReader = @Login";
        } else
            if (login.Length == 19)
            {
                for (int i = 0; i < 19; i++)
                {
                    if (!int.TryParse(login[i].ToString(), out NumberReader))
                    {
                        Check = true;
                        break;
                    }
                }
                if (!Check)//значит 18 цифр. типа номер социалки вбил
                {
                    CommandText = "select * from Readers..Main where NumberSC = @Login";
                }
            }
            else //не номер, и не социалка, значит email
            {
                CommandText = "select * from Readers..Main where Email = @Login";
            }


        SqlDataAdapter da = new SqlDataAdapter();
        da.SelectCommand = new SqlCommand();
        da.SelectCommand.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReadersConnection_OnlyRead"].ConnectionString);
        da.SelectCommand.Parameters.AddWithValue("Login", login);
        da.SelectCommand.CommandText = CommandText;
        DataSet ds = new DataSet();
        int cnt = da.Fill(ds);
        if (cnt == 0) throw new Exception("Читатель не найден!");
        if (cnt > 1) throw new Exception("Таких Email найдено больше одного! Введите номер читателя!");

        string Salt = ds.Tables[0].Rows[0]["WordReg"].ToString();

        string HashedPwd = HashPass(password, Salt);

        da.SelectCommand.Parameters.Clear();
        da.SelectCommand.Parameters.AddWithValue("NumberReader", ds.Tables[0].Rows[0]["NumberReader"].ToString());
        da.SelectCommand.Parameters.AddWithValue("Password", HashedPwd);
        da.SelectCommand.CommandText = "select * from Readers..Main where NumberReader = @NumberReader and Password = @Password ";
        ds = new DataSet();
        cnt = da.Fill(ds);

        if (cnt == 0) throw new Exception("Неверный пароль.");

        return true;
    }

    public String HashPass(String strPassword, String strSol)
    {
        String strHashPass = String.Empty;
        byte[] bytes = Encoding.Unicode.GetBytes(strSol + strPassword);
        //создаем объект для получения средст шифрования 
        SHA256CryptoServiceProvider CSP = new SHA256CryptoServiceProvider();
        //вычисляем хеш-представление в байтах 
        byte[] byteHash = CSP.ComputeHash(bytes);
        //формируем одну цельную строку из массива 
        foreach (byte b in byteHash)
        {
            strHashPass += string.Format("{0:x2}", b);
        }
        return strHashPass;
    }

    //[WebMethod(Description = "Восстановление пароля. Возвращает true при успешном восстановлении пароля. Во всех остальных случаях генерируется исключение.")]
    public bool PasswordRecovery(string login, string familyName, string name, string fatherName, string dateBirth, string pwd1, string pwd2)
    {

        if (pwd1 != pwd2) throw new Exception("Пароль и его подтверждение не совпадают!");
        string CommandText = "";
        int NumberReader = 0;
        bool Check = false;
        CultureInfo provider = CultureInfo.InvariantCulture;
        DateTime DateBirth;
        if (!DateTime.TryParseExact(dateBirth, "dd.MM.yyyy", provider, DateTimeStyles.None, out DateBirth)) throw new Exception("Неверный формат даты.");

        if (login.Length == 6 && int.TryParse(login, out NumberReader))
        {
            CommandText = "select * from Readers..Main where NumberReader = @Login";
        }
        else
            if (login.Length == 19)
            {
                for (int i = 0; i < 19; i++)
                {
                    if (!int.TryParse(login[i].ToString(), out NumberReader))
                    {
                        Check = true;
                        break;
                    }
                }
                if (!Check)//значит 18 цифр. типа номер социалки вбил
                {
                    CommandText = "select * from Readers..Main where NumberSC = @Login";
                }
            }
            else //не номер, и не социалка, значит email
            {
                CommandText = "select * from Readers..Main where Email = @Login";
            }

        SqlDataAdapter da = new SqlDataAdapter();
        da.SelectCommand = new SqlCommand();
        da.SelectCommand.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReadersConnection_OnlyRead"].ConnectionString);
        da.SelectCommand.Parameters.AddWithValue("Login", login);
        da.SelectCommand.CommandText = CommandText;
        DataSet ds = new DataSet();
        int cnt = da.Fill(ds);
        if (cnt == 0) throw new Exception("Читатель не найден!");
        if (cnt > 1) throw new Exception("Таких Email найдено больше одного! Необходимо указать номер читателя номер читателя!");


        NumberReader = Convert.ToInt32(ds.Tables[0].Rows[0]["NumberReader"]);


        da.SelectCommand.Parameters.Clear();
        da.SelectCommand.Parameters.AddWithValue("NumerReader", NumberReader);
        da.SelectCommand.Parameters.AddWithValue("FamilyName", familyName);
        da.SelectCommand.Parameters.AddWithValue("Name", name);
        da.SelectCommand.Parameters.AddWithValue("DateBirth", DateBirth);

        if (fatherName == "")
        {
            da.SelectCommand.CommandText = "select * from Readers.dbo.Main where FamilyName = @FamilyName and [Name] = @Name and FatherName is null and  DateBirth = @DateBirth and NumberReader = @NumberReader";
        }
        else
        {
            da.SelectCommand.Parameters.AddWithValue("FatherName", fatherName);
            da.SelectCommand.CommandText = "select * from Readers.dbo.Main where FamilyName = @FamilyName and [Name] = @Name and FatherName = @FatherName and  DateBirth = @DateBirth and NumberReader = @NumberReader";
        }

        ds = new DataSet();
        int hit = da.Fill(ds, "hit");
        if (hit == 0)
        {
            throw new Exception("Введённые данные не найдены!");
        }

        da.UpdateCommand = new SqlCommand();
        da.UpdateCommand.Connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ReadersConnection_OnlyReadWrite"].ConnectionString);
        da.UpdateCommand.Connection.Open();

        da.UpdateCommand.Parameters.Add("pass", SqlDbType.NVarChar);
        da.UpdateCommand.Parameters.Add("wordreg", SqlDbType.NVarChar);
        string wordreg = RndWordReg(32);
        string passw = HashPass(pwd1, wordreg);
        da.UpdateCommand.Parameters["pass"].Value = passw;
        da.UpdateCommand.Parameters["wordreg"].Value = wordreg;
        da.UpdateCommand.Parameters.AddWithValue("NumberReader", NumberReader);
        da.UpdateCommand.CommandText = "update Readers..Main set Password = @pass,WordReg = @wordreg where NumberReader = @NumberReader";
        int y = da.UpdateCommand.ExecuteNonQuery();
        da.UpdateCommand.Connection.Close();
        return true;
    }
    [WebMethod(Description = "Получить тип логина. Возвращает NumberReader, если введён номер читательского билета. "+
                             " Возвращает SocialCardNumber если введён номер социальной карты. Возвращает Email если введён Email. "+
                             " В остальных случаях возвращает NotDefined.")]
    public string GetLoginType(string login)
    {
        int NumberReader = 0;
        bool Check = false;
        if (login.Length == 6 && int.TryParse(login, out NumberReader))
        {
            return "NumberReader";
        }
        else
            if (login.Length == 19)
            {
                for (int i = 0; i < 19; i++)
                {
                    if (!int.TryParse(login[i].ToString(), out NumberReader))
                    {
                        Check = true;
                        break;
                    }
                }
                if (!Check)//значит 18 цифр. типа номер социалки вбил
                {
                    return "SocialCardNumber";
                }
            }
            else
                if (Regex.IsMatch(login,
                   @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$"))//не номер, и не социалка, значит email. проверяем формат
                {
                    return "Email";
                }
                else
                {
                    return "NotDefined";
                }
        return "NotDefined";
    }
    public String RndWordReg(int nLength)
    {
        String WordReg = "";
        byte[] bRandom = new byte[1];
        RNGCryptoServiceProvider Gen = new RNGCryptoServiceProvider();
        int i = 0;
        while (i < nLength)
        {
            Gen.GetBytes(bRandom);
            if (((bRandom[0] >= 48) && (bRandom[0] <= 57)) || ((bRandom[0] >= 65) && (bRandom[0] <= 90)) ||
                ((bRandom[0] >= 97) && (bRandom[0] <= 122)))
            {
                WordReg += Convert.ToChar(bRandom[0]);
                i++;
            }
        }
        return WordReg;
    }
}