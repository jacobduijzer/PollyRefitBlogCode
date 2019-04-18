using System;

namespace PollyRefitTest.Customers
{
    public class Customer
    {
        public readonly Guid Id;

        public readonly string FullName;

        public readonly string Email;

        public Customer(string fullName, string email)
        {
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
        }
    }
}