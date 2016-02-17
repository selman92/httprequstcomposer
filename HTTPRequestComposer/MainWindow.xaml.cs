using System;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HTTPRequestComposer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cmbMethods.ItemsSource = Enum.GetNames(typeof(HttpRequestMethod));
            cmbMethods.SelectedValue = "GET";

            txtGeneral.Background = Brushes.WhiteSmoke;
            txtHeaders.Background = Brushes.WhiteSmoke;
            txtResponse.Background = Brushes.WhiteSmoke;
        }
   
        private void btnYes_Checked(object sender, RoutedEventArgs e)
        {
            txtRawRequest.IsEnabled = true;

            txtAcceptedTypes.IsEnabled = false;
            txtAcceptEncoding.IsEnabled = false;
            txtAcceptLanguage.IsEnabled = false;
            txtUri.IsEnabled = false;
            txtUserAgent.IsEnabled = false;
            cmbMethods.IsEnabled = false;
            
        }

        private void btnNo_Checked(object sender, RoutedEventArgs e)
        {
            if (txtRawRequest != null)
            {
                txtRawRequest.IsEnabled = false;

                txtAcceptedTypes.IsEnabled = true;
                txtAcceptEncoding.IsEnabled = true;
                txtAcceptLanguage.IsEnabled = true;
                txtUri.IsEnabled = true;
                txtUserAgent.IsEnabled = true;
                cmbMethods.IsEnabled = true;
            }

        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            txtGeneral.Text = "";
            txtHeaders.Text = "";
            txtResponse.Text = "";

            lblStatus.Content = "Starting to send request...";
            var composer = new HttpRequestComposer();
            bool result;
            if (btnYes.IsChecked.GetValueOrDefault())
            {
                var rawRequest = txtRawRequest.Text;
                lblStatus.Content = "Parsing the raw request...";
                result = composer.ParseRawRequest(rawRequest);
                if (!result)
                {
                    lblStatus.Content =
                        string.Format("An error has been occurred while parsing the raw request string. Details: {0}",
                            composer.ErrorMessage);
                }
            }
            else
            {
                lblStatus.Content = "Validating the form...";
                result = composer.ValidateForm(txtUri.Text,txtHost.Text,cmbMethods.SelectedValue.ToString());
                if (!result)
                {
                    lblStatus.Content =
                        string.Format("An error has been occurred while validating the form values. Details: {0}",
                            composer.ErrorMessage);
                }
                else
                {
                    composer.InitializeHttpRequest(txtUri.Text,txtHost.Text,cmbMethods.SelectedValue.ToString(),txtUserAgent.Text,txtAcceptedTypes.Text,txtAcceptEncoding.Text,txtAcceptLanguage.Text);
                }
            }

            if (result)
            {
                lblStatus.Content = "Sending the request...";
                result = await composer.SendRequest();
                if (!result)
                {
                    lblStatus.Content =
                        string.Format("An error has been occurred while sending the request. Details: {0}",
                            composer.ErrorMessage);
                }
                else
                {
                    lblStatus.Content = "Request has been completed succesfully.";
                    WriteGeneralInformation(composer.Response);
                    WriteHeaders(composer.Response);
                    txtResponse.Text = composer.ResponseHtml;
                    //scrollViewer.UpdateLayout();
                }
            }
        }

       

        private void WriteGeneralInformation(HttpWebResponse response)
        {
            txtGeneral.Inlines.Add(new Bold(new Run("Status Code: ")));
            txtGeneral.Inlines.Add(((int)response.StatusCode).ToString());
            txtGeneral.Inlines.Add(" - " + response.StatusCode.ToString());
            txtGeneral.Inlines.Add(Environment.NewLine);

            txtGeneral.Inlines.Add(new Bold(new Run("Method: ")));
            txtGeneral.Inlines.Add(response.Method);
            txtGeneral.Inlines.Add(Environment.NewLine);

            txtGeneral.Inlines.Add(new Bold(new Run("Response Url: ")));
            txtGeneral.Inlines.Add(response.ResponseUri.ToString());
            txtGeneral.Inlines.Add(Environment.NewLine);

            txtGeneral.Inlines.Add(new Bold(new Run("Server: ")));
            txtGeneral.Inlines.Add(response.Server);
            txtGeneral.Inlines.Add(Environment.NewLine);

            if (response.CharacterSet != null)
            {
                txtGeneral.Inlines.Add(new Bold(new Run("Character Set: ")));
                txtGeneral.Inlines.Add(response.CharacterSet);
                txtGeneral.Inlines.Add(Environment.NewLine);
            }
        }

        private void WriteHeaders(HttpWebResponse response)
        {
            for (int i = 0; i < response.Headers.Count; ++i)
            {
                txtHeaders.Inlines.Add(new Bold(new Run(string.Format("{0}:", response.Headers.Keys[i]))));
                txtHeaders.Inlines.Add(response.Headers[i]);
                txtHeaders.Inlines.Add(Environment.NewLine);
            }
            
           
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtAcceptedTypes.Text = "";
            txtAcceptEncoding.Text = "";
            txtAcceptLanguage.Text = "";
            txtHost.Text = "";
            txtRawRequest.Text = "";
            txtUri.Text = "";
            txtUserAgent.Text = "";
           
        }
    }
}
