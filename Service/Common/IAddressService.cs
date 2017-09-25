using InSearch.Core.Domain.Common;
using InSearch.Core.Domain.Users;

namespace InSearch.Services.Common
{
    /// <summary>
    /// Address service interface
    /// </summary>
    public partial interface IAddressService
    {
        User GetUserByAddressId(int addressId);

        /// <summary>
        /// Deletes an address
        /// </summary>
        /// <param name="address">Address</param>
        void DeleteAddress(Address address);

		/// <remarks>codehint: sm-add</remarks>
		void DeleteAddress(int id);

        /// <summary>
        /// Gets total number of addresses by country identifier
        /// </summary>
        /// <param name="countryId">Country identifier</param>
        /// <returns>Number of addresses</returns>
        int GetAddressTotalByCountryId(int countryId);

        /// <summary>
        /// Gets an address by address identifier
        /// </summary>
        /// <param name="addressId">Address identifier</param>
        /// <returns>Address</returns>
        Address GetAddressById(int addressId);

        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        void InsertAddress(Address address);

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        void UpdateAddress(Address address);

        /// <summary>
        /// Gets a value indicating whether address is valid (can be saved)
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>Result</returns>
        bool IsAddressValid(Address address);
    }
}