using Newtonsoft.Json;
using SumiAPI.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SumiAPI.Models
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                string authenticationToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedAuthenticationToken = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationToken));
                string[] usernamePasswordArray = decodedAuthenticationToken.Split(':');
                string uname = usernamePasswordArray[0];
                string pass = usernamePasswordArray[1];
                bool isAuthenticated = false;
                using (StreamReader r = new StreamReader(HttpContext.Current.Server.MapPath("~/Content/Json/user.json")))
                {
                    string json = r.ReadToEnd();
                    List<User> users = JsonConvert.DeserializeObject<List<User>>(json);
                    foreach (User usr in users)
                    {
                        if (usr.Username.Equals(uname) && usr.Password.Equals(pass))
                        {
                            isAuthenticated = true;
                            var currentPrincipal = new GenericPrincipal(new GenericIdentity(usr.Username), null);
                            actionContext.RequestContext.Principal = currentPrincipal;
                            Thread.CurrentPrincipal = currentPrincipal;
                            HttpContext.Current.User = currentPrincipal;
                        }
                    }
                }
                if (!isAuthenticated)
                {
                    // Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(uname), null);
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

            }
        }
    }
}