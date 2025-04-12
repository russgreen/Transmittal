using ClosedXML.Report.Utils;

namespace Transmittal.Reports.Models;

public class ProjectDirectoryReportModel : Transmittal.Library.Models.ProjectDirectoryModel
{
    public string CompanyName
    {
        get
        {
            return Company.CompanyName;
        }
    }

    public string Role
    {
        get
        {
            return Company.Role;
        }
    }

    public string Address
    {
        get
        {
            return Company.Address;
        }
    }

    public string Tel
    {
        get
        {
            return Company.Tel;
        }
    }

    public string Fax
    {
        get
        {
            return Company.Fax;
        }
    }

    public string Website
    {
        get
        {
            return Company.Website;
        }
    }

    public string LastName
    {
        get
        {
            return Person.LastName;
        }
    }

    public string FirstName
    {
        get
        {
            return Person.FirstName;
        }
    }

    public string Email
    {
        get
        {
            return Person.Email;
        }
    }

    public string DDI
    {
        get
        {
            return Person.Tel;
        }
    }

    public string Mobile
    {
        get
        {
            return Person.Mobile;
        }
    }

    public string Position
    {
        get
        {
            return Person.Position;
        }
    }


    public string CompanyRoleAndName
    {
        get
        {

        }
    }

    public string CompanyContactDetails
    {
        get
        {
            string returnValue;

            returnValue = $"{CompanyName}\n";

            if(Role != null)
            {
                returnValue = $"{returnValue} ({Role})\n";
            }

            if(Tel != null)
            {
                returnValue = $"{returnValue}Tel:{Tel}\n";
            }

            if(Website != null)
            {
                returnValue = $"{returnValue}WWW:{Website}\n";
            }

            if(Address != null)
            {
                returnValue = $"{returnValue}{Address}\n";
            }

            return returnValue;
        }
    }
}
