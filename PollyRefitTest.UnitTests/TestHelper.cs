using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Bogus;
using PollyRefitTest.Customers;
using PollyRefitTest.Orders;
using PollyRefitTest.Products;
using Refit;

namespace PollyRefitTest.UnitTests
{
    public static class TestHelper
    {
        public static ApiException CreateRefitException(HttpMethod method, HttpStatusCode httpStatusCode) =>
            ApiException.Create(
                new HttpRequestMessage(),
                method,
                new HttpResponseMessage(httpStatusCode)).Result;

        public static Customer CreateFakeCustomer() =>
            new Faker<Customer>()
                .CustomInstantiator(faker => new Customer(faker.Person.FullName, faker.Person.Email))
                .Generate();

        public static List<Product> CreateFakeProducts(int amount)
        {
            Randomizer.Seed = new Random(8675309);

            return new Faker<Product>()
                .RuleFor(product => product.Id, (faker, product) => Guid.NewGuid())
                .RuleFor(product => product.Name, (faker, product) => faker.Commerce.ProductName())
                .RuleFor(product => product.Description, (faker, product) => faker.Commerce.ProductAdjective())
                .RuleFor(product => product.ImageUrl, (faker, product) => $"https://www.somesite.com/product/{faker.Commerce.ProductName()}")
                .RuleFor(product => product.Price, (faker, product) => double.Parse(faker.Commerce.Price()))
                .Generate(amount);
        }

        public static Order CreateFakeOrder(int numberOfProducts)
        {
            var random = new Bogus.Randomizer();
            var fakeOrder = new Faker<Order>()
                .CustomInstantiator(Faker => new Order(CreateFakeCustomer()))
                .Generate();

            var fakeProducts = CreateFakeProducts(numberOfProducts);
            fakeProducts.ForEach(product =>
            {
                fakeOrder.AddOrderItem(new OrderItem(product, random.Number(1, 8)));
            });

            return fakeOrder;
        }
    }
}
