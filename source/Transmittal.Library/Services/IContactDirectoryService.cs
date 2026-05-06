using System.Collections.ObjectModel;
using Transmittal.Library.Models;

namespace Transmittal.Library.Services;
public interface IContactDirectoryService
{
    /// <summary>
    /// Get a list of all companies 
    /// </summary>
    /// <returns>A list of CompanyModel</returns>
    List<CompanyModel> GetCompanies_All();
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
    /// Delete a company
    /// </summary>
    /// <param name="companyID"></param>
    void DeleteCompany(CompanyModel model);
    /// <summary>
    /// Get a list of all contacts within a company
    /// </summary>
    /// <param name="model"></param>
    /// <returns>A list of PersonModel</returns>
    List<PersonModel> GetPeople_ByCompany(int companyID);
    /// <summary>
    /// Get a list of all people
    /// </summary>
    /// <returns>A list of PersonModel</returns>
    List<PersonModel> GetPeople_All();
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
    /// Find potential duplicate or similar companies by name.
    /// </summary>
    /// <param name="companyName">Company name to compare against existing entries.</param>
    /// <param name="maxResults">Maximum number of matches to return.</param>
    /// <returns>Ordered list of likely matches (best match first).</returns>
    List<CompanyModel> FindCompanyMatches(string companyName, int maxResults = 5);

    /// <summary>
    /// Find potential duplicate or similar people by name/email and optional company.
    /// </summary>
    /// <param name="firstName">First name to compare.</param>
    /// <param name="lastName">Last name to compare.</param>
    /// <param name="email">Email to compare.</param>
    /// <param name="companyID">Optional company ID to boost matching relevance.</param>
    /// <param name="maxResults">Maximum number of matches to return.</param>
    /// <returns>Ordered list of likely matches (best match first).</returns>
    List<PersonModel> FindPersonMatches(string firstName, string lastName, string email, int? companyID = null, int maxResults = 5);
    /// <summary>
    /// Update a contact
    /// </summary>
    /// <param name="model"></param>
    void UpdatePerson(PersonModel model);
    /// <summary>
    /// Delete a contact
    /// </summary>
    /// <param name="model"></param>
    void DeletePerson(PersonModel model);

    
    ///// <summary>
    ///// Get the project directory list 
    ///// </summary>
    ///// <returns>List of ProjectDirectoryModel</returns>
    List<ProjectDirectoryModel> GetProjectDirectory(bool IncludeArchivedUsers = true);
}
