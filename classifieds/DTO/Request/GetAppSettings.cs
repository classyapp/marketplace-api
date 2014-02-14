using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Classy.Models;
using ServiceStack.FluentValidation;

namespace Classy.Models.Request
{
    public class GetAppSettings : BaseRequestDto
    {
        public int AppId { get; set; }
    }

    public class GetAppSettingsValidator : AbstractValidator<GetAppSettings>
    {
        public GetAppSettingsValidator()
        {

        }
    }
}