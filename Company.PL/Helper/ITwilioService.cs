

using Company.Ali.DAL.Models.Sms;
using Twilio.Rest.Api.V2010.Account;

namespace Company.Ali.PL.Helper
{
    public interface ITwilioService
    {
        public MessageResource SendSms(Sms sms);


    }
}
