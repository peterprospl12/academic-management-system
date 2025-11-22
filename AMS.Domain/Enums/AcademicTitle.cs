using System.ComponentModel;

namespace AMS.Domain.Enums;

public enum AcademicTitle
{
    [Description("")] None = 0,

    [Description("mgr")] Master = 1,

    [Description("dr")] Doctor = 2,

    [Description("dr hab.")] HabilitatedDoctor = 3,

    [Description("prof.")] Professor = 4
}