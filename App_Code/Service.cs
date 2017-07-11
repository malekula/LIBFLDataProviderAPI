using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;



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
        return ri;
    }
    
}
