using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WebApi.Models;
using WooCommerceApi.Models.WooCommerceModels;
using WooCommerceApi.Utilities;
using System.Web;

namespace WooCommerceApi.Helpers
{
    public static class WooCommerceConverters
    {
        public static WooAttribute ToWooAttribute(WebApi.Models.Attribute webAttribute)
        {
            return new WooAttribute
            {
                Id = webAttribute.Id,
                Name = webAttribute.Name,
                Options = new List<string> { webAttribute.Value },
                Terms = new List<WooAttributeTerm>
                {
                    new WooAttributeTerm
                    {
                        Name = webAttribute.Value
                    }
                }
            };
        }
        public static WooProduct ToWooProduct(WebProduct webProduct)
        {
            return new WooProduct
            {
                Id = webProduct.Id,
                Name = webProduct.Name,
                Description = webProduct.Description,
                RegularPrice = ((int)webProduct.RegularPrice).ToString(CultureInfo.InvariantCulture),
                SalePrice = ((int)webProduct.RegularPrice).ToString(CultureInfo.InvariantCulture),
                ManageStock = "true",
                StockQuantity = webProduct.StockQuantity.ToString(),
                Sku = webProduct.Sku,
                Categories = webProduct.Categories?.Select(c => new MinimalWooCategory
                {
                    Id = c.Id
                }).ToList(),
                Status = "draft"
            };
        }
        public static WooProduct ToWooVariableProduct(WebProduct webProduct)
        {
            return new WooProduct
            {
                Id = webProduct.Id,
                Name = webProduct.Name,
                Description = webProduct.Description,
                ManageStock = "true",
                StockQuantity = webProduct.StockQuantity.ToString(),
                Sku = webProduct.Sku,
                Categories = webProduct.Categories?.Select(c => new MinimalWooCategory
                {
                    Id = c.Id
                }).ToList(),
                Status = "draft",
                Type = "variable",
                Attributes = new List<WooAttribute>(),
                //Attributes = webProduct?.Attributes?.Select(a => new WooAttribute
                //{
                //    Id = a.Id,
                //    Name = a.Name,
                //    Option = a.Value,

                //})?.ToList() ?? new List<WooAttribute>()
            };
        }
        public static WooProductVariation ToWooVariationProduct(WebProduct webProduct)
        {
            return new WooProductVariation
            {
                Id = webProduct.Id,
                Name = webProduct.Name,
                RegularPrice = ((int)webProduct.RegularPrice).ToString(CultureInfo.InvariantCulture),
                SalePrice = ((int)webProduct.RegularPrice).ToString(CultureInfo.InvariantCulture),
                ManageStock = "true",
                StockQuantity = webProduct.StockQuantity.ToString(),
                Sku = webProduct.Sku,
                Type = "variation",
            };
        }

        public static WebProduct ToWebProduct(WooProduct wooProduct)
        {
            var webProduct = new WebProduct
            {
                Id = wooProduct.Id,
                Name = wooProduct.Name,
                DateCreated = wooProduct.DateCreated ?? DateTime.Now,
                DateModified = wooProduct.DateModified ?? DateTime.Now,
                // Status = wooProduct.Status,
                Description = wooProduct.Description,
                Sku = wooProduct.Sku,
                RegularPrice = TryToDecimal(wooProduct.RegularPrice),
                SalePrice = TryToNullableDecimal(wooProduct.SalePrice),
                StockQuantity = TryToDouble(wooProduct.StockQuantity),
                Categories = wooProduct.Categories?.Select(c => new WebCategory
                {
                    Id = c.Id
                }).ToList().ToList(),
                Attributes = new List<WebApi.Models.Attribute>()

            };

            if (wooProduct.Attributes != null)
            {
                foreach (var attribute in wooProduct.Attributes)
                {
                    if (attribute.Options == null)
                        continue;

                    foreach (var option in attribute.Options)
                    {
                        webProduct.Attributes.Add(new WebApi.Models.Attribute
                        {
                            Id = attribute.Id,
                            Name = HttpUtility.UrlDecode(attribute.Name),
                            Value = HttpUtility.UrlDecode(option),
                        });
                    }
                }
            }
            if (wooProduct.Type == "variable" && wooProduct.VariationIds != null)
            {
                webProduct.Variations = wooProduct.VariationIds.Select(id => new WebProduct
                {
                    Id = id
                }).ToList();
            }
            return webProduct;
        }
        public static WebProduct VariationToWebProduct(WooProduct wooProduct)
        {
            var webProduct = new WebProduct
            {
                Id = wooProduct.Id,
                Name = wooProduct.Name,
                DateCreated = wooProduct.DateCreated ?? DateTime.Now,
                DateModified = wooProduct.DateModified ?? DateTime.Now,
                Description = wooProduct.Description,
                Sku = wooProduct.Sku,
                RegularPrice = TryToDecimal(wooProduct.RegularPrice),
                SalePrice = TryToNullableDecimal(wooProduct.SalePrice),
                StockQuantity = TryToDouble(wooProduct.StockQuantity),
                Categories = wooProduct.Categories?.Select(c => new WebCategory
                {
                    Id = c.Id
                }).ToList().ToList(),
                Attributes = wooProduct.Attributes?.Select(a => new WebApi.Models.Attribute
                {
                    Id = a.Id,
                    Name = HttpUtility.UrlDecode(a.Name),
                    Value = HttpUtility.UrlDecode(a.Option),
                }).ToList() ?? new List<WebApi.Models.Attribute>()

            };
            return webProduct;
        }

        public static WooCategory ToWooCategory(WebCategory webCategory)
        {
            return new WooCategory
            {
                Id = webCategory.Id,
                Name = webCategory.Name,
                Description = webCategory.Description,
                Parent = webCategory.ParentId,
            };
        }
        
        public static WebCustomer ToWebCustomer(WooCustomer wooCustomer)
        {
            return new WebCustomer
            {
                Id = wooCustomer.Id,
                FirstName = wooCustomer.FirstName,
                LastName = wooCustomer.LastName,
                Email = wooCustomer.Email,
                PhoneNumbers = new List<string>
                {
                    wooCustomer.Phone  
                },
                Address1 = wooCustomer.Address1,
                Address2 = wooCustomer.Address2,
                City = wooCustomer.City,
                State = wooCustomer.State,
                Postcode = wooCustomer.Postcode,
                Country = wooCustomer.Country,
                CreatedDate = wooCustomer.DateCreated ?? DateTime.Now,
            };
        }

        private static int? TryToNullableInt(object value)
        {
            int.TryParse(value?.ToString(), out int result);
            if (result == 0)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        public static WebCategory ToWebCategory(WooCategory wooCategory)
        {
            return new WebCategory
            {
                Id = wooCategory.Id,  // Assigns 0 if Id is null
                Name = wooCategory.Name,
                Description = wooCategory.Description,
                ParentId = wooCategory.Parent
            };
        }

        public static WebOrder ToWebOrder(WooOrder wooOrder)
        {
            var orderStatus = Constants.OrderStatuses
                .FirstOrDefault(x => x.Value == wooOrder.Status)
                .Key;
            if (!Constants.OrderStatuses.Any(o => o.Value == wooOrder.Status))
            {
                orderStatus = OrderStatus.Other;
            }

            double totalTax = wooOrder.TotalTax;
            double sumOfSubTaxes = wooOrder.LineItems.Sum(x => x.SubtotalTax);
            bool isTaxAfterDiscount = totalTax != sumOfSubTaxes;
            
            var firstShippingLine = wooOrder.ShippingLines?.FirstOrDefault();

            return new WebOrder
            {
                Id = wooOrder.Id,
                OrderPath = string.Format(Constants.OrderPath, wooOrder.Id),
                CustomerId = wooOrder.CustomerId != null && wooOrder.CustomerId != "0" ? wooOrder.CustomerId : null,
                CustomerNote = wooOrder.CustomerNote,
                PaymentMethod = new WebPaymentMethod
                {
                    Id = wooOrder.PaymentMethod,
                    Title = wooOrder.PaymentMethodTitle,
                },
                TransactionId = wooOrder.TransactionId,
                Status = orderStatus,
                StatusText = wooOrder.Status,
                IsConverted = false,
                DateCreated = wooOrder.DateCreated ?? DateTime.Now,
                DateModified = wooOrder.DateModified ?? DateTime.Now,
                Currency = wooOrder.Currency,
                ShippingTotal = wooOrder.ShippingTotal,
                Billing = MapCustomer(wooOrder.Billing),
                Shipping = MapCustomer(wooOrder.Shipping),
                ShippingDetail = new WebShippingDetail
                {
                    VehicleId = firstShippingLine?.MethodId,
                    VehicleName = firstShippingLine?.MethodTitle,
                    VehiclePrice = double.TryParse(firstShippingLine?.Total, out double shippingTotal) ? shippingTotal : 0
                },
                OrderItems = wooOrder.LineItems.Select(item => new WebOrderDetail
                {
                    Name = item.Name,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Quantity == 0 ? item.Subtotal : item.Subtotal / item.Quantity,
                    VariationId = item.VariationId != null && item.VariationId != "0" ? item.VariationId : null,
                    UnitDiscount = (item.Subtotal - item.Total) / item.Quantity,
                    UnitTax = (isTaxAfterDiscount ? item.TotalTax : item.SubtotalTax) / item.Quantity
                }).ToList()
            };
        }
        private static WebCustomer MapCustomer(WooCustomer customer)
        {
            // Get country name from dictionary, if not found assign the original value
            string country = Constants.CountryDictionary.TryGetValue(customer.Country, out string countryName)
                             ? countryName : customer.Country;

            // Get state name from dictionary, if not found assign the original value
            string state = Constants.StateDictionary.TryGetValue(customer.State, out string stateName)
                           ? stateName : customer.State;

            return new WebCustomer
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumbers = new List<string> { customer.Phone },
                Address1 = customer.Address1,
                Address2 = customer.Address2,
                Country = country,
                State = state,
                City = customer.City,
                Postcode = customer.Postcode
            };
        }

        public static WebPaymentMethod ToWebPaymentMethod(WooPaymentMethod wooPaymentMethod)
        {
            return new WebPaymentMethod
            {
                Id = wooPaymentMethod.Id,
                Title = wooPaymentMethod.Title,
                Description = wooPaymentMethod.Description
            };
        }
        internal static int TryToInt(object value)
        {
            return int.TryParse(value?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }
        internal static double TryToDouble(object value)
        {
            return double.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        internal static decimal TryToDecimal(object value)
        {
            return decimal.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0m;
        }

        internal static decimal? TryToNullableDecimal(object value)
        {
            return decimal.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : (decimal?)null;
        }
        public static string GenerateSlug(this string phrase) 
        { 
            string str = phrase.RemoveAccent().ToLower(); 
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); 
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim(); 
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();   
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str; 
        }

        public static string RemoveAccent(this string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}