﻿using System.Collections.Generic;

namespace MdsCommon
{
    public class InputCredentials
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SignInPage
    {
        public string ErrorMessage { get; set; }
        public string LoginMessage { get; set; } = "Sign in to your account";
        public string ReturnUrl { get; set; }

        public InputCredentials Credentials { get; set; } = new();
    }

    public class ListInfrastructureEventsPage : IHasUser
    {
        public List<InfrastructureEvent> InfrastructureEvents { get; set; } = new();
        public InfrastructureEvent SelectedEvent { get; set; }
        public bool ShowSidePanel { get; set; }
        public User User { get; set; } = new();

        public static InfrastructureEvent NoEvent = new InfrastructureEvent() { Id = System.Guid.Empty };
    }

    public static partial class Header
    {
        public class Title
        {
            public string Operation { get; set; }
            public string Entity { get; set; }
        }

        public class Props
        {
            public Title Main { get; set; }
            public List<Title> Secondary { get; set; } = new List<Title>();
            public User User { get; set; }
            public bool UseSignIn { get; set; } = true;
        }
    }

}
