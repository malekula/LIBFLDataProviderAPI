using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using DataProviderAPI.Queries;
using System.Configuration;

/// <summary>
/// Сводное описание для BookLoader
/// </summary>
public class BJBookLoader
{
    public BJBookLoader(string baseName)
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

    public BookInfo GetBJBookByID(int iDMAIN)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BookStatusConnection"].ConnectionString;
        string queryText = new BJBooksQueries(this.BaseName).GET_BOOK_BY_IDMAIN;
        DataTable result = new DataTable();
        SqlDataAdapter da = new SqlDataAdapter(queryText, connectionString);
        da.SelectCommand.Parameters.Add("IDMAIN", SqlDbType.Int).Value = iDMAIN;
        da.Fill(result);
        BookInfo bi = new BookInfo();
        string fieldCode;
        foreach(DataRow row in result.Rows)
        {
            fieldCode = row["MNFIELD"].ToString()+row["MSFIELD"].ToString();
            switch (fieldCode)//пока только автор заглавие
            {
                case "200$a":
                    bi.Title = row["PLAIN"].ToString();
                    break;
                case "700$a":
                case "701$a":
                    bi.Author = row["PLAIN"].ToString();
                    break;
                case "330$a":
                    bi.Annotation = row["PLAIN"].ToString();
                    break;
            }
        }
        return bi;
    }
    public BookInfo GetBJBookByINV(string invNumber)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BookStatusConnection"].ConnectionString;
        string queryText = new BJBooksQueries(this.BaseName).GET_IDMAIN_BY_INVNUMBER;
        DataTable result = new DataTable();
        SqlDataAdapter da = new SqlDataAdapter(queryText, connectionString);
        da.SelectCommand.Parameters.Add("InvNumber", SqlDbType.NVarChar).Value = invNumber;
        da.Fill(result);
        if (result.Rows.Count == 0) return null;
        int IDMAIN = (int)result.Rows[0]["IDMAIN"];
        return this.GetBJBookByID(IDMAIN);
    }

}
