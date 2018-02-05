using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using GithubUserInfo.Jsons;
using GithubUserInfo.UserControls;

namespace GithubUserInfo.Windows
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private HttpWebRequest request;
        private HttpWebResponse response;
        private string mainurl = "https://api.github.com";
        private string useragent = "GithubUserInfo";

        private User user_json = new User();
        private List<FFUser> ffusers_json = new List<FFUser>();
        private List<Repository> repositories_json = new List<Repository>();
        private List<Repository> stars_json = new List<Repository>();
        private List<Repository> watches_json = new List<Repository>();
        private List<Organization> organizations_json = new List<Organization>();
        private string userinfo;
        private string ffinfo;
        private string reposlist;
        private string stars;
        private string watches;
        private string organizations;

        private static string pleasewait = "お待ちください";
        private static string loading = "読み込み中です...";
        private bool showid_flag = false;
        private string access_token;

        public MainWindow() => InitializeComponent();

        private async void btn_search_Click(object sender, RoutedEventArgs e) => await SearchUserAsync(textbox_search.Text);

        private void textbox_forsearch_TextChanged(object sender, TextChangedEventArgs e) => btn_search.IsEnabled = textbox_search.Text == string.Empty || textbox_accesstoken.Text == string.Empty ? false : true;

        private void label_username_MouseDown(object sender, MouseButtonEventArgs e)
        {
            label_username.Content = showid_flag ? user_json.login : $"ID: {user_json.id}";
            showid_flag = !showid_flag;
        }

        private async void btn_more_Click(object sender, RoutedEventArgs e) => await ShowMoreUserInfoAsync(user_json.login);

        private async void label_followingcount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowFFUserAsync(user_json.following_url);

        private async void label_followerscount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowFFUserAsync(user_json.followers_url);

        private async void label_repositoriescount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowRepositoriesAsync(user_json.repos_url);

        private async void label_starredcount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowRepositoriesAsync(user_json.starred_url);

        private async void label_watchescount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowRepositoriesAsync(user_json.subscriptions_url);

        private async void label_organizationscount_MouseDown(object sender, MouseButtonEventArgs e) => await ShowOrganizationsAsync();

        /// <summary>
        /// 指定したユーザーを検索します。
        /// </summary>
        private async Task SearchUserAsync(string username, bool istextboxclear = false)
        {
            access_token = textbox_accesstoken.Text;
            request = WebRequest.Create($"{mainurl}/users/{username}?access_token={access_token}") as HttpWebRequest;
            request.UserAgent = useragent;
            request.Method = "GET";

            ProgressDialogController searchprogress = default;

            try
            {
                searchprogress = await this.ShowProgressAsync(pleasewait, loading);

                response = await request.GetResponseAsync() as HttpWebResponse;

                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    userinfo = await reader.ReadToEndAsync();
                }

                request = WebRequest.Create($"{mainurl}/users/{username}/starred?access_token={access_token}") as HttpWebRequest;
                request.UserAgent = useragent;
                request.Method = "GET";

                response = await request.GetResponseAsync() as HttpWebResponse;

                using(var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    stars = await reader.ReadToEndAsync();
                }

                request = WebRequest.Create($"{mainurl}/users/{username}/subscriptions?access_token={access_token}") as HttpWebRequest;
                request.UserAgent = useragent;
                request.Method = "GET";

                response = await request.GetResponseAsync() as HttpWebResponse;

                using(var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    watches = await reader.ReadToEndAsync();
                }

                request = WebRequest.Create($"{mainurl}/users/{username}/orgs?access_token={access_token}") as HttpWebRequest;
                request.UserAgent = useragent;
                request.Method = "GET";

                response = await request.GetResponseAsync() as HttpWebResponse;

                using(var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream);
                    organizations = await reader.ReadToEndAsync();
                }

                await ShowUserInfoAsync(username);

                await searchprogress.CloseAsync();

                if (istextboxclear) textbox_search.Clear();

                btn_more.IsEnabled = true;
                label_followingcount.IsEnabled = true;
                label_followerscount.IsEnabled = true;
                label_repositoriescount.IsEnabled = true;
                label_starredcount.IsEnabled = true;
                label_watchescount.IsEnabled = true;

                goto END;
            }
            catch (WebException err)
            {
                await this.ShowMessageAsync(err.Status.ToString(), $"指定されたユーザーは見つかりません。以下の項目を確認してください。{Environment.NewLine}・ユーザー名が間違っていないか{Environment.NewLine}・ユーザー名ではなく、名前を指定していないか");
                MessageBox.Show(err.ToString());
            }

            await searchprogress.CloseAsync();

            END:;
        }

        /// <summary>
        /// 検索で見つかったユーザーの情報を表示します。
        /// </summary>
        private async Task ShowUserInfoAsync(string username)
        {
            user_json = JsonConvert.DeserializeObject<User>(userinfo);
            stars_json = JsonConvert.DeserializeObject<Repository[]>(stars).ToList();
            watches_json = JsonConvert.DeserializeObject<Repository[]>(watches).ToList();
            organizations_json = JsonConvert.DeserializeObject<Organization[]>(organizations).ToList();

            if (!string.IsNullOrEmpty(user_json.avatar_url)) img_icon.Source = await GetImageAsync(user_json.avatar_url);
            label_name.Content = user_json.name;
            label_username.Content = user_json.login;
            textbox_bio.Text = user_json.bio;
            label_followingcount.Content = user_json.following.ToString() ?? "-";
            label_followerscount.Content = user_json.followers.ToString() ?? "-";
            label_repositoriescount.Content = user_json.public_repos.ToString() ?? "-";
            label_starredcount.Content = stars_json.Count;
            label_watchescount.Content = watches_json.Count;
            label_organizationscount.Content = organizations_json.Count;
        }

        /// <summary>
        /// 非同期で画像ダウンロードを行います。
        /// 参考: https://chitoku.jp/programming/wpf-lazy-image-behavior
        /// </summary>
        private Task<BitmapImage> GetImageAsync(string url)
        {
            return Task.Run(() =>
            {
                var wc = new WebClient { CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable) };
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = new MemoryStream(wc.DownloadData(url));
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch (WebException) { }
                catch (IOException) { }
                catch (InvalidOperationException) { }
                finally
                {
                    wc.Dispose();
                }
                return null;
            });
        }

        /// <summary>
        /// "もっと見る..."を押したときに表示する
        /// </summary>
        private async Task ShowMoreUserInfoAsync(string username)
        {
            var progress = await this.ShowProgressAsync(pleasewait, loading);

            var moreprofilewindow = new MoreProfileWindow();

            if (!string.IsNullOrEmpty(user_json.avatar_url)) moreprofilewindow.image_icon.Source = await GetImageAsync(user_json.avatar_url);
            moreprofilewindow.label_name.Content = user_json.name;
            moreprofilewindow.label_username.Content = user_json.login;
            moreprofilewindow.label_id.Content = $"ID: {user_json.id}";
            moreprofilewindow.label_company.Content = $"Company: {user_json.company}";
            moreprofilewindow.label_location.Content = $"Location: {user_json.location}";
            moreprofilewindow.label_email.Content = $"Email: {user_json.email}";
            moreprofilewindow.textblock_blogurl.Text = user_json.blog;
            if (!string.IsNullOrEmpty(user_json.blog)) moreprofilewindow.hyperlink_blogurl.NavigateUri = new Uri(user_json.blog);
            moreprofilewindow.textblock_htmlurl.Text = user_json.html_url;
            moreprofilewindow.hyperlink_htmlurl.NavigateUri = new Uri(user_json.html_url);
            moreprofilewindow.label_type.Content = $"Type: {user_json.type}";
            moreprofilewindow.label_isadmin.Content = $"IsAdmin: {user_json.site_admin}";
            moreprofilewindow.label_hireable.Content = $"Hireable: {user_json.hireable}";
            moreprofilewindow.textbox_bio.Text = user_json.bio;
            moreprofilewindow.label_createdat.Content = $"Created at {user_json.created_at.ToLocalTime()}";
            moreprofilewindow.label_updatedat.Content = $"Updated at {user_json.updated_at.ToLocalTime()}";

            moreprofilewindow.Show();

            await progress.CloseAsync();
        }

        /// <summary>
        /// フォロー&フォロワーユーザーを取得&表示します。
        /// </summary>
        private async Task ShowFFUserAsync(string url)
        {
            var ffwindow = new FFWindow();

            request = WebRequest.Create($"{url}?access_token={access_token}".Replace("{/other_user}", string.Empty)) as HttpWebRequest;
            request.UserAgent = useragent;
            request.Method = "GET";

            var progress = await this.ShowProgressAsync(pleasewait, loading);

            response = await request.GetResponseAsync() as HttpWebResponse;

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                ffinfo = await reader.ReadToEndAsync();
            }

            ffusers_json = JsonConvert.DeserializeObject<FFUser[]>(ffinfo).ToList();

            foreach (var ffuser in ffusers_json)
            {
                var ffbtn = new FFButton();
                ffbtn.image_icon.Source = await GetImageAsync(ffuser.avatar_url);
                ffbtn.label_username.Content = ffuser.login;
                ffbtn.MouseDoubleClick += async (ff_sender, ff_e) =>
                {
                    await SearchUserAsync(ffbtn.label_username.Content.ToString(), true);
                    ffwindow.Close();
                };

                ffwindow.ff_panel.Children.Add(ffbtn);
            }

            ffwindow.label_ff.Content = url.Contains("following") ? "フォロー一覧" : "フォロワー一覧";
            ffwindow.Show();

            await progress.CloseAsync();
        }

        /// <summary>
        /// リポジトリ一覧を取得&表示します。
        /// </summary>
        private async Task ShowRepositoriesAsync(string url)
        {
            var repositorieswindow = new RepositoriesWindow();

            if (url.Contains("starred"))
            {
                url = url.Replace("{/owner}{/repo}", string.Empty);
                repositorieswindow.Title = "Star一覧";
            }
            else if(url.Contains("subscriptions"))
            {
                repositorieswindow.Title = "Watch一覧";
            }
            else
            {
                repositorieswindow.Title = "リポジトリ一覧";
            }

            request = WebRequest.Create($"{url}?access_token={access_token}") as HttpWebRequest;
            request.UserAgent = useragent;
            request.Method = "GET";

            var progress = await this.ShowProgressAsync(pleasewait, loading);

            response = (HttpWebResponse)request.GetResponse();

            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                reposlist = await reader.ReadToEndAsync();
            }

            repositories_json = JsonConvert.DeserializeObject<Repository[]>(reposlist).ToList();

            foreach(var repository in repositories_json)
            {
                var repositorybtn = new RepositoryViewer();
                repositorybtn.ToolTip = $"Created at {repository.created_at.ToLocalTime()}{Environment.NewLine}Updated at {repository.updated_at.ToLocalTime()}Pushed at {repository.pushed_at.ToLocalTime()}";
                repositorybtn.label_reponame.Content = repository.name;
                repositorybtn.textbox_description.Text = repository.description;
                repositorybtn.label_language.Content = repository.language;
                repositorybtn.label_starscount.Content = repository.stargazers_count;
                repositorybtn.label_forkscount.Content = repository.forks_count;

                repositorieswindow.repos_panel.Children.Add(repositorybtn);
            }

            repositorieswindow.Show();

            await progress.CloseAsync();
        }

        private async Task ShowOrganizationsAsync()
        {
            var organizationswindow = new OrganizationsWindow();

            request = WebRequest.Create(user_json.organizations_url) as HttpWebRequest;
            request.UserAgent = useragent;
            request.Method = "GET";

            var progress = await this.ShowProgressAsync(pleasewait, loading);

            response = await request.GetResponseAsync() as HttpWebResponse;

            using(var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream);
                organizations = await reader.ReadToEndAsync();
            }

            organizations_json = JsonConvert.DeserializeObject<Organization[]>(organizations).ToList();

            foreach(var organization in organizations_json)
            {
                var organizationbtn = new OrganizationButton();
                organizationbtn.image_icon.Source = await GetImageAsync(organization.avatar_url);
                organizationbtn.label_orgname.Content = organization.login;
                organizationbtn.textbox_description.Text = organization.description;

                organizationswindow.organizations_panel.Children.Add(organizationbtn);
            }

            organizationswindow.Show();

            await progress.CloseAsync();
        }
    }
}
