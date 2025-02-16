using DiscoverUO.Lib.Shared.Data;
using System.Xml.Serialization;
using System.ComponentModel;
using DiscoverUO.Lib.Shared.Users;

namespace DiscoverUO.Desktop
{
    public partial class DiscoverUOMainForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SessionManager UserSession { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HttpClient SessionClient { get; set; }

        public DiscoverUOMainForm()
        {
            UserSession = new SessionManager();
            SessionClient = new HttpClient();
            SessionClient.BaseAddress = new Uri("http://localhost:5219");

            InitializeComponent();
        }

        private void mainMenuAuthenticationButton_Click(object sender, EventArgs e)
        {
            AuthenticationData data = new AuthenticationData();

            data.Username = "Admin";
            data.Password = "toast7S$";

            var rsp = UserSession.UserSignIn(data, SessionClient);

            MessageBox.Show(rsp.Message);
        }

        private void DiscoverUOMainForm_Load(object sender, EventArgs e)
        {

        }

        private void mainMenuDashboardButton_Click(object sender, EventArgs e)
        {

        }
    }
}
