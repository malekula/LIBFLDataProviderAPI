using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для ReaderInfo
/// </summary>
public class ReaderInfo
{
    public string FamilyName { get; set; }
    public string Name { get; set; }
    public string FatherName { get; set; }
    public DateTime DateBirth { get; set; }
    public bool IsRemoteReader { get; set; }
}
