using System;
using System.Collections.Generic;
using System.Text;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public class ContactDirectoryService : IContactDirectoryService
{
    public ContactDirectoryService()
    {
    }

    public void CreateCompany(CompanyModel model)
    {
        throw new NotImplementedException();
    }

    public void CreatePerson(PersonModel model)
    {
        throw new NotImplementedException();
    }

    public void CreateProjectDirectory(ProjectDirectoryModel model)
    {
        throw new NotImplementedException();
    }

    public void DeleteProjectDirectory(ProjectDirectoryModel model)
    {
        throw new NotImplementedException();
    }

    public List<CompanyModel> GeCompanies_All()
    {
        throw new NotImplementedException();
    }

    public CompanyModel GetCompany(int companyID)
    {
        throw new NotImplementedException();
    }

    public List<PersonModel> GetContacts_ByCompany(int companyID)
    {
        throw new NotImplementedException();
    }

    public PersonModel GetPerson(int personID)
    {
        throw new NotImplementedException();
    }

    public List<ProjectDirectoryModel> GetProjectDirectory()
    {
        throw new NotImplementedException();
    }

    public void UpdateCompany(CompanyModel model)
    {
        throw new NotImplementedException();
    }

    public void UpdatePerson(PersonModel model)
    {
        throw new NotImplementedException();
    }

    public void UpdateProjectDirectory(ProjectDirectoryModel model)
    {
        throw new NotImplementedException();
    }
}
