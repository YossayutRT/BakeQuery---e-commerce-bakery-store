using System;
using System.Collections.Generic;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class LabStudent
{
    public string StdID { get; set; } = null!;

    public string StdPASSWORD { get; set; } = null!;

    public string? StdName { get; set; }

    public string? StdLastname { get; set; }
}