using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

/// <summary>
/// Сводное описание для BookInfo
/// </summary>

public class BookInfo
{
    public string Title { get; set; }


    private List<string> _author = new List<string>();
    public string Author 
    {
        get
        {
            StringBuilder result = new StringBuilder();
            foreach (string author in _author)
            {
                result.Append(author);
                result.Append("; ");
            }
            return result.ToString().Remove(result.Length-2);
        }
        set
        {
            _author.Add(value);
        }
    }

    
}

