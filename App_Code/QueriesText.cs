using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataProviderAPI.Queries
{
    public class BJBooksQueries
    {
        private string _dbName;

        public BJBooksQueries(string DBName)
        {
            this._dbName = DBName;
        }


        public string GET_BOOK_BY_IDMAIN 
        {
            get 
            {
                return 
                        "select A.MNFIELD, A.MSFIELD , B.PLAIN from " + this._dbName + "..DATAEXT A " +
                        "left join BJVVV..DATAEXTPLAIN B on A.ID = B.IDDATAEXT " +
                        "where A.IDMAIN = @IDMAIN";
            }
        }

        public string GET_IDMAIN_BY_INVNUMBER
        {
            get 
            {
                return
                        "select IDMAIN from "+this._dbName+"..DATAEXT "+
                        "where MNFIELD = 899 and MSFIELD = '$p' and SORT = @InvNumber";
            }
        }
    }

    public class ReadersQueries
    {
        //public static
    }
}