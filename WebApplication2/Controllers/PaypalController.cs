using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebApplication2.helper;

namespace WebApplication2.Controllers
{


    public class PaypalController : Controller
    {
        private PayPal.Api.Payment payment;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal()
        {
            APIContext apiContext = Configuration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority
                    +
                    "/Paypal/PaymentWithPayPal?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error" + ex.Message);
                return View("FailureView");
            }
            return View("SuccessView");
        }

        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var itemList = new ItemList() { items = new List<Item>(), shipping_address = new ShippingAddress() { recipient_name = "Nguyễn Văn Tèo", country_code="VN", city="Hồ Chí Minh", line1="72 Nguyễn Hữu Cảnh, F22, Q.Bình Thạnh", postal_code="700000"} };
            //recipient_name: tên người đặt hàng
            //country_code: code quốc gia, tham khảo thêm tại: https://developer.paypal.com/docs/api/reference/country-codes/
            //city: thành phố shipping
            //line1: địa chỉ giao hàng
            //postal_code: code postal (ví dụ code ở Việt Nam: https://www.google.com/search?q=postal+code+vietnam)
            itemList.items.Add(new Item()
            {
                //Thông tin đơn hàng
                name = "Item Name",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
            });         
            var payer = new Payer() { payment_method = "paypal" };
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            var details = new Details()
            {
                tax = "1",
                shipping = "2",
                subtotal = "5"
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = "8", 
                details = details
            };
            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.", //nội dung thanh toán
                invoice_number = DateTime.Now.ToString(), //mã hóa đơn
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            return this.payment.Create(apiContext);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

    }

    
}
