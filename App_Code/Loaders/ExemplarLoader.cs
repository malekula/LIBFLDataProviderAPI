using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using DataProviderAPI.Queries;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Drawing;
using Newtonsoft.Json;

/// <summary>
/// Сводное описание для ExemplarLoader
/// </summary>
public class ExemplarLoader
{
    public ExemplarLoader(string baseName)
	{
        this._baseName = baseName;
	}

    public string BaseName
    {
        get
        {
            return _baseName;
        }
    }
    private string _baseName;
    public ExemplarInfo GetExemplarInfoByIdData(int IDDATA)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BookStatusConnection"].ConnectionString;
        string queryText = new BJExemplarQueries(this.BaseName).GET_EXEMPLAR_BY_IDDATA;
        DataTable result = new DataTable();
        SqlDataAdapter da = new SqlDataAdapter(queryText, connectionString);
        da.SelectCommand.Parameters.Add("IDDATA", SqlDbType.Int).Value = IDDATA;
        da.Fill(result);
        ExemplarInfo ei = new ExemplarInfo(IDDATA);
        string fieldCode;
        foreach (DataRow row in result.Rows)
        {
            fieldCode = row["MNFIELD"].ToString() + row["MSFIELD"].ToString();
            switch (fieldCode)//пока только автор заглавие
            {
                case "899$a":
                    ei.Location = row["PLAIN"].ToString();
                    break;
                case "899$p":
                    ei.InventoryNumber = row["PLAIN"].ToString();
                    break;
                case "899$w":
                    ei.Barcode = row["PLAIN"].ToString();
                    break;
            }
        }
        return ei;
    }
    //[WebMethod(Description = "Информация об электронной копии. Принимает строку с id книги из VuFind (например BJVVV_123456)")]
    public ExemplarInfo GetElectronicExemplarInfo(string id)
    //временно получаем так, пока не будут проинвентаризированы электронные копии
    {
        string ip = ConfigurationManager.ConnectionStrings["IPAddressFileServer"].ConnectionString;
        string login = ConfigurationManager.ConnectionStrings["LoginFileServer"].ConnectionString;
        string pwd = ConfigurationManager.ConnectionStrings["PasswordFileServer"].ConnectionString;
        string _directoryPath = @"\\" + ip + @"\BookAddInf\";

        ExemplarInfo result = new ExemplarInfo(-1);
        FileInfo[] fi;
        using (new NetworkConnection(_directoryPath, new NetworkCredential("BJStor01\\imgview", "Image_123Viewer")))
        {
            _directoryPath = @"\\" + ip + @"\BookAddInf\" + ElectronicCopyInfo.GetPathToElectronicCopy(id);
            DirectoryInfo di = new DirectoryInfo(_directoryPath);
            fi = di.GetFiles("*.jpg");
            foreach (FileInfo f in fi)
            {
                result.JPGFiles.Add(f.FullName.Substring(f.FullName.IndexOf("BookAddInf")+11 ));
            }
            result.Path = di.FullName.Substring(di.FullName.IndexOf("BookAddInf") + 11);
            
        }
        Image img = Image.FromFile(fi[0].FullName);
        result.WidthFirstFile = img.Width;
        result.HeightFirstFile = img.Height;
        result.IsElectronicCopy = true;
        return result;
        //return JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
    }
}
