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

        public DiscoverUOMainForm()
        {
            UserSession = new SessionManager();
            InitializeComponent();
        }

        private void mainMenuAuthenticationButton_Click(object sender, EventArgs e)
        {
            AuthenticationData data = new AuthenticationData();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("http://localhost:5219");

            data.Username = "Admin";
            data.Password = "toast7S$";

            var rsp = UserSession.UserSignIn(data, client);

            MessageBox.Show(rsp.Message);
        }

        private void DiscoverUOMainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
