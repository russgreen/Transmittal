﻿using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public interface IContactDirectoryService
{
    /// <summary>
    /// Get a list of all companies 
    /// </summary>
    /// <returns>A list of CompanyModel</returns>
    List<CompanyModel> GeCompanies_All();
    /// <summary>
    /// Get a single company
    /// </summary>
    /// <param name=""></param>
    /// <returns>CompanyModel</returns>
    CompanyModel GetCompany(int companyID);
    /// <summary>
    /// Create a new Company
    /// </summary>
    /// <param name="model"></param>
    void CreateCompany(CompanyModel model);
    /// <summary>
    /// Update a company
    /// </summary>
    /// <param name="model"></param>
    void UpdateCompany(CompanyModel model);
    /// <summary>
    /// Get a list of all contacts within a company
    /// </summary>
    /// <param name="companyID"></param>
    /// <returns>A list of PersonModel</returns>
    List<PersonModel> GetContacts_ByCompany(int companyID);
    /// <summary>
    /// Get a single approved list contact 
    /// </summary>
    /// <param name="personID"></param>
    /// <returns>PersonModel</returns>
    PersonModel GetPerson(int personID);
    /// <summary>
    /// Create a new contact
    /// </summary>
    /// <param name="model"></param>
    void CreatePerson(PersonModel model);
    /// <summary>
    /// Update a contact
    /// </summary>
    /// <param name="model"></param>
    void UpdatePerson(PersonModel model);
    

    /// <summary>
    /// Get the project directory list 
    /// </summary>
    /// <returns>List of ProjectDirectoryModel</returns>
    List<ProjectDirectoryModel> GetProjectDirectory();
    /// <summary>
    /// Create a new entry in the project directory
    /// </summary>
    /// <param name="model"></param>
    void CreateProjectDirectory(ProjectDirectoryModel model);
    /// <summary>
    /// Update an entry in the project directoruy
    /// </summary>
    /// <param name="model"></param>
    /// <remarks>should only be changing the ShowInReport entry else use UpdatePerson()</remarks>
    void UpdateProjectDirectory(ProjectDirectoryModel model);
    /// <summary>
    /// Delete an entry from the project directory.
    /// </summary>
    /// <param name="model"></param>
    /// <remarks>cannot delete an entry if documents have been issued to them</remarks>
    void DeleteProjectDirectory(ProjectDirectoryModel model);
}
