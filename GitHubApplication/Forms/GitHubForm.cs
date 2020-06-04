﻿using Unity;
using System;
using System.Linq;
using System.Drawing;
using GitHub.Core.Models;
using System.Windows.Forms;
using System.Threading.Tasks;
using GitHubApplication.Common;
using GitHubApplication.Controls;
using GitHubApplication.Properties;
using GitHub.Core.Services.Abstractions;
using Unity.Resolution;

namespace GitHubApplication
{
    public partial class GitHubForm : Form
    {
        private readonly IUserManager UserManager;
        private readonly IGitHubApiService GitHubService;
        private readonly SearchLanguageCategoriesControl[] LanguageComboBoxControls;

        private User _User;
        public User User
        {
            get => _User;
            set
            {
                // ვავსებთ შესული მომხმარებლის ინფორმაციით ლეიბლებს.
                _User = value;
            }
        }

        // პანელების დაფარვა და გამოჩენა, შეიძლება ამის შეცვლა.
        private bool ShowMainPanel
        {
            set
            {
                MainPanel.Visible = value;
                UserRoomPanel.Visible = false;
                ComparisonPanel.Visible = false;
            }
        }
        private bool ShowUserRoomPanel
        {
            set
            {
                UserRoomPanel.Visible = value;
                MainPanel.Visible = false;
                ComparisonPanel.Visible = false;
            }
        }
        private bool ShowComparisonPanel
        {
            set
            {
                ComparisonPanel.Visible = value;
                MainPanel.Visible = false;
                UserRoomPanel.Visible = false;
            }
        }

        public GitHubForm(IGitHubApiService gitHubService, IUserManager userManager)
        {
            InitializeComponent();

            GitHubService = gitHubService;
            UserManager = userManager;

            // TODO: ენები ბაზაში უნდა დაემატოს და მერე LINQ-ით ამოიღებ და შექმნი
            LanguageComboBoxControls = new SearchLanguageCategoriesControl[]
            {
                new SearchLanguageCategoriesControl("C", LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("C++",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("C#",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("Java",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("Python",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("Swift",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("JavaScript",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("PHP",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("HTML",LanguageButtonClickHandler),
                new SearchLanguageCategoriesControl("CSS",LanguageButtonClickHandler)
            };

            LanguageComboBoxPanel.Controls.AddRange(LanguageComboBoxControls);
        }

        private void GitHubForm_Load(object sender, EventArgs e)
        {
            TransferToLoginForm();
        }

        private void TransferToLoginForm()
        {
            var loginFrom = ServiceManager.Instance.Container.Resolve<LoginForm>();
            loginFrom.SuccessfullyLogged += SuccessfullyLoggedHandler;

            if (loginFrom.ShowDialog() == DialogResult.Cancel)
            {
                Close();
                return;
            }
            else
            {
                // თუ რამე უნდა ჩაიტვირთოს ავტომატურად შესვლისთანავე ვტვირთავთ აქ.
            }
        }

        //TODO: ცოტა შესაცვლელია
        private void LanguageButtonClickHandler(object sender, string languageName)
        {
            ChooseLanguageButton.Text = languageName;
            LanguageChoosen = true;
            LanguageComboBoxPanel.Visible = false;
        }

        private bool LanguageChoosen = false; //TODO ესეც შესაცვლელია.
        private async void SearchButton_Click(object sender, EventArgs e)
        {
            ShowMainPanel = true;

            ClearPanelControls(MainPanel);

            if (!string.IsNullOrEmpty(SearchForRepositoryesTextBox.Text) && LanguageChoosen)
            {
                var repositories = await GitHubService.SearchForRepositories(ChooseLanguageButton.Text, default(DateTime), SearchForRepositoryesTextBox.Text);
                var controls = await GetRepositoryControls(repositories);

                MainPanel.Controls.AddRange(controls);
            }
        }

        private async void TrendingRepositoriesButton_Click(object sender, EventArgs e)
        {
            LoadingPictureBox.Image = Resources.LoadingAnimationForRepositories;
            LoadingPictureBox.Visible = true;

            ShowMainPanel = true;

            ClearPanelControls(MainPanel);

            var repositories = await GitHubService.TrendingRepositories();
            var controls = await GetRepositoryControls(repositories);
            MainPanel.Controls.AddRange(controls);

            LoadingPictureBox.Visible = false;
        }

        private async void TrendingDevelopersButton_Click(object sender, EventArgs e)
        {
            LoadingPictureBox.Image = Resources.LoadingAnimationForDevelopers;
            LoadingPictureBox.Visible = true;

            ShowMainPanel = true;

            ClearPanelControls(MainPanel);

            var developers = await GitHubService.TrendingDevelopers();
            var controls = await GetDeveloperControls(developers);
            MainPanel.Controls.AddRange(controls);

            LoadingPictureBox.Visible = false;
        }

        private void ClearPanelControls(Panel panel) // ...
        {
            if (panel.Controls.Count > 0)
                panel.Controls.Clear();
        }

        // ამას არ ვიყენებთ მარა იყოს.
        private Task<TControl[]> GetControls<Tsource, TControl>(Tsource[] repositories, Func<Tsource, TControl> creator)
        {
            return Task.Run(() => repositories.Select(creator).ToArray());
        }

        private Task<RepositoryControl[]> GetRepositoryControls(Repository[] repositories)
        {
            return Task.Run(() => repositories.Select(r => new RepositoryControl(r)).ToArray());
        }

        private Task<DeveloperControl[]> GetDeveloperControls(User[] developers)
        {
            return Task.Run(() => developers.Select(u => new DeveloperControl(u)).ToArray());
        }

        private void ComparisionRepositoriesButton_Click(object sender, EventArgs e)
        {
            ShowComparisonPanel = true;
            ClearPanelControls(ComparisonPanel);

            ComparisonPanel.Controls.Add(new RepositoriesComparisonControl(GitHubService));
        }

        private void ChooseLanguageButton_Click(object sender, EventArgs e)
        {
            LanguageComboBoxPanel.Visible = !LanguageComboBoxPanel.Visible;
        }

        public void SuccessfullyLoggedHandler(object sender, User user)
        {
            User = UserManager.FindUser(user.UserName);

            if (sender is LoginForm loginForm)
                loginForm.SuccessfullyLogged -= SuccessfullyLoggedHandler;
        }

        private void TopButtons_MouseHover(object sender, EventArgs e)
        {
            if (sender is PictureBox topButtonPictureBox)
            {
                topButtonPictureBox.BackColor = Color.FromKnownColor(KnownColor.DimGray);
            }
        }

        private void TopButtons_MouseLeave(object sender, EventArgs e)
        {
            if (sender is PictureBox topButtonPictureBox)
            {
                topButtonPictureBox.BackColor = Color.Transparent;
            }
        }

        private void MinimizeButton_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void CloseButton_Click(object sender, EventArgs e) => Close();

        private void UserRoomButton_Click(object sender, EventArgs e)
        {
            ShowUserRoomPanel = true;
            ClearPanelControls(UserRoomPanel);

            var userRoom = ServiceManager.Instance.Container.Resolve<UserRoomControl>(new ParameterOverride("user", User));
            userRoom.UserDeactivated += UserDeactivatedHandler;
            UserRoomPanel.Controls.Add(userRoom);
        }

        public void UserDeactivatedHandler(object sender, User user)
        {
            UserManager.DeactivateUser(user);
            TransferToLoginForm();

            if (sender is UserRoomControl userRoomControl)
                userRoomControl.UserDeactivated -= UserDeactivatedHandler;
        }


        // ესენი მისაბმელია.
        private bool mouseDown = false;
        private Point lastFormLocation;
        private void BankForm_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastFormLocation = e.Location;
        }

        private void BankForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Location = new Point(
                    (Location.X - lastFormLocation.X) + e.X,
                    (Location.Y - lastFormLocation.Y) + e.Y);
                Update();
            }
        }

        private void BankForm_MouseUp(object sender, MouseEventArgs e) => mouseDown = false;
    }
}