using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CarTradingWeb.Database.Repositories;
using CarTradingWeb.Dto;
using CarTradingWeb.Service;
using NSubstitute;
using CarTradingWeb.Model;
using Xunit;
using CarTradingWeb.Model.Enum;
using CarTradingWeb.Exceptions;

namespace CarTradingWebTests.Service
{
    public class VatServiceTests
    {
        private readonly IVatRepository repository;
        private readonly IVatService service;
        public VatServiceTests()
        {
            repository = Substitute.For<IVatRepository>();
            service = new VatService(Substitute.For<IMapper>(), repository);
        }
        [Theory]
        [InlineData(5000)]
        [InlineData(27500)]
        [InlineData(100000)]
        public async void GetVatPriceAsync_ShouldReturnModifiedPrice_WhenAdHasVatIsTrue(int price)
        {
            //Arrange
            var dummyVat = new Vat()
            {
                CountryCode = (Enums.CountryCode)0,
                Id = 0,
                Rate = 25,
                RateMultiplier = 1.25M
            };
            repository.GetVatByCountryCodeAsync(0).Returns(Task.FromResult<Vat>(dummyVat));
            var dummyAd = new AdDto()
            {
                HasVat = true,
                AdPrice = price
            };

            //Act
            var resultPrice = await service.GetVatPriceAsync(0, dummyAd);
            var expectedPrice = (int)(price * dummyVat.RateMultiplier);
            //Assert
            Assert.Equal(expectedPrice, resultPrice);
        }

        [Fact]
        public async void GetVatPricesAsync_ShouldInvokeGetVat_WhenHasVatIsTrue()
        {
            //Arrange
            var dummyAd = new AdDto()
            {
                AdTitle = "Test Ad",
                HasVat = true,
                AdPrice = 5
            };
            var argToRecieve = 500;
            repository.GetVatByCountryCodeAsync(argToRecieve).Returns(Task.FromResult<Vat>(new Vat()));

            //Act
            var sut = await service.GetVatPriceAsync(argToRecieve, dummyAd);

            //Assert
            await repository.Received().GetVatByCountryCodeAsync(Arg.Is<int>(x => x == argToRecieve));
        }

        [Fact]
        public async void GetVatPricesAsync_ShouldThrowInvalidAdException_WhenNullAdArgument()
        {
            await Assert.ThrowsAsync<InvalidAdException>(() 
                => service.GetVatPriceAsync(Arg.Any<int>(), null));
        }

        [Fact]
        public async void GetVatPricesAsync_ShouldThrowUnknownVatException_WhenRepositoryReturnsNullVatAndAdHasVat()
        {
            repository.GetVatByCountryCodeAsync(Arg.Any<int>()).ReturnsForAnyArgs(Task.FromResult<Vat>(null));

            await Assert.ThrowsAsync<UnknownVatException>(()
                => service.GetVatPriceAsync(Arg.Any<int>(), new AdDto() { HasVat = true}));
        }
    }
}
