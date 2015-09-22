using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TrDictionary.Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await Dictionary.Load();
        }

        private async void ButtonFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = Dictionary.Search(TextBoxKey.Text, RadioButtonExactMatch.IsChecked.Value);
                if (result.Count > 0)
                {
                    TextBlockUnknown.Visibility = Visibility.Collapsed;
                    ListViewResults.ItemsSource = result;
                    ListViewResults.Visibility = Visibility.Visible;
                    WebViewResults.Visibility = Visibility.Collapsed;
                }
                else
                {

                    HttpClient client = new HttpClient();
                    var body = new HttpStringContent(string.Format("kelime={0}&kategori=verilst&ayn=tam&gonder=ARA", TextBoxKey.Text), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                    var response = await client.PostAsync(new Uri("http://tdk.gov.tr/index.php?option=com_bts&arama=kelime&guid=TDK.GTS.560196ef4acf19.06533032", UriKind.Absolute), body);
                    var htmlcon = await response.Content.ReadAsStringAsync();
                    if (htmlcon.Contains("bulunamadı"))
                    {
                        NotFound();
                    }
                    else
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(htmlcon);
                        var decs = doc.DocumentNode.Descendants();
                        foreach (var item in decs)
                        {
                            var attr = item.Attributes["class"];
                            if (attr != null)
                            {
                                if (attr.Value == "main_body")
                                {
                                    for (int i = 0; i < 19; i++)
                                    {
                                        item.RemoveChild(item.FirstChild);

                                    }
                                    foreach (var elem in item.Descendants())
                                    {
                                        if (elem.Name == "a")
                                        {
                                            var att = elem.Attributes["href"];
                                            if (att != null)
                                            {
                                                att.Value = "";
                                            }
                                        }
                                    }
                                    item.InnerHtml += "<a>Sonuçlar http://tdk.gov.tr/ üzerinden sağlanmaktadır.</a>";
                                    WebViewResults.NavigateToString(item.InnerHtml);

                                    ListViewResults.Visibility = Visibility.Collapsed;
                                    WebViewResults.Visibility = Visibility.Visible;

                                    TextBlockUnknown.Visibility = Visibility.Collapsed;
                                    break;

                                }
                            }
                        }

                    }

                }
            }
            catch
            {
                NotFound();
            }
        }

        private void NotFound()
        {
            TextBlockUnknown.Visibility = Visibility.Visible;
            ListViewResults.Visibility = Visibility.Collapsed;
            WebViewResults.Visibility = Visibility.Collapsed;
        }
    }
}
