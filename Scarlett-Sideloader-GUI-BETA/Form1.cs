﻿using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;
//using System.Net.Http;
using HtmlAgilityPack;


namespace Scarlett_Sideloader_GUI_BETA
{
    public partial class Form1 : Form
    {
        public static Uri partneruri = new Uri("https://partner.microsoft.com/");
        public static Uri xboxuri = new Uri("https://upload.xboxlive.com/");
        public static Uri xbluri = new Uri("https://xorc.xboxlive.com/");
        public static Dictionary<String, String> Groups = new Dictionary<string, string>();

        public static string StorePage;

        public static HttpClient client;

        PublisherInfo publisherinfo;
        public Form1()
        {
            InitializeComponent();
            GroupBoxes.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.Add(partneruri, new Cookie(".AspNet.Cookies", aspcookies.Text));
            HttpClientHandler handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                Proxy = new WebProxy("http://localhost:8080", false),
                UseProxy = false //change to true for debug
            };
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            client = new HttpClient(handler);

            GroupBoxes.Items.Clear();
            Groups.Clear();
            List<NeededGroupInfo> groups = GetAllGroups();
            publisherinfo = retryFunction<PublisherInfo>(() => GetPublisherInfo(), 3);
            if (groups == null || publisherinfo == null)
            {
                MessageBox.Show("Failed to get groups or publisher info, token may be invalid", "Login err", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                GroupBoxes.Enabled = false;
                upload.Enabled = false;
                appPrivate.Enabled = false;
                appPublic.Enabled = false;
                return;
            }
            foreach (NeededGroupInfo group in groups)
            {
                GroupBoxes.Items.Add(group.name);
                Groups.Add(group.name, group.id);
            }
            GroupBoxes.Enabled = true;
            upload.Enabled = true;
            appPrivate.Enabled = true;
            appPublic.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "App Files|*.appx;*.appxbundle;*.msix;*.msixbundle;*.appxupload;*.msixupload";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                appfile.Text = openFileDialog1.FileName;
            }
        }
        
        private void upload_Click(object sender, EventArgs e)
        {
            List<NeededGroupInfo> Neededgroups = new List<NeededGroupInfo>();

            foreach (var item in GroupBoxes.CheckedItems)
            {
                string test = item.ToString();
                Neededgroups.Add(new NeededGroupInfo() { id = Groups[test], name = test });
            }
            if (namerandomised.Checked)
                AppName.Text = RandomString(16);
            
            SideloaderMain( AppName.Text, appfile.Text, RandomString(32), "blank.png", Neededgroups);
        }

        public static bool retryFunction(Func<bool> function, int attempts)
        {
            for (int i = 0; i < attempts; i++)
            {
                if (function())
                    return true;
            }
            return false;
        }

        public static T retryFunction<T>(Func<T> function, int attempts)
        {
            T output;
            for (int i = 0; i < attempts; i++)
            {
                output = function();
                if (output != null)
                    return output;
            }
            return default(T);
        }
        public void WriteLine(string text, ConsoleColor color)
        {
            if (color == ConsoleColor.Red)
            {
                MessageBox.Show(text, "ERR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (color == ConsoleColor.Green)
            { 
                //skip - no point just writing success over and over again
            }
            else
            {
                statusmessage.Text = text;
            }
        }

        public string SideloaderMain(string appname, string filepath, string appdescription, string appscreenshotname, List<NeededGroupInfo> Neededgroups)
        {
            string filename = Path.GetFileName(filepath);

            

            //get all needed group info
            //create empty lists of groups and emails
             
            
            AppInfo createappinfo = new AppInfo()
            {
                Name = appname,
                features = new AppFeatures()
                {
                    //game = !commandArguments.app
                    game = game.Checked
                }
            };

            statusmessage.Text = ($"Creating APP with name {appname}");
            NeededAppInfo createdappinfo = retryFunction<NeededAppInfo>(() => CreateApp(createappinfo), 3);
            if (createdappinfo == null)
            {
                WriteLine("Failed to create app, name is likely already taken", ConsoleColor.Red);
                return null;
            }

            /*if (commandArguments.name != null)
            {

                bool succeeded = false;
                while (!succeeded)
                {
                    statusmessage.Text = ($"Checking availability of {commandArguments.name}");
                    bool? available = retryFunction<bool?>(() => CheckAvailability(commandArguments.name), 3);
                    if (available == default(bool?))
                    {
                        WriteLine("Failed to check availability of app name", ConsoleColor.Red);
                        return null;
                    }
                    else if (available == true)
                    {
                        WriteLine("Success!", ConsoleColor.Green);
                        succeeded = true;
                    }
                    else
                    {
                        if (commandArguments.force)
                        {
                            WriteLine($"Name was not available, adjusting and trying again", ConsoleColor.Yellow);
                            commandArguments.name = RandomInvisibleString(3) + commandArguments.name + RandomInvisibleString(3);
                        }
                        else
                        {
                            WriteLine("Name is not available", ConsoleColor.Red);
                            return null;
                        }
                    }
                }
                statusmessage.Text = ($"Reserving app name ({commandArguments.name})");
                if (retryFunction(() => ReserveAppName(createdappinfo, commandArguments.name), 3))
                {
                    WriteLine("Success!", ConsoleColor.Green);
                }
                else
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                /*Console.ForegroundColor = ConsoleColor.White;
                statusmessage.Text = ($"Switching app name to {commandArguments.name}");
                if (SwitchAppName(createdappinfo, commandArguments.name))
                {
                    WriteLine("Success!", ConsoleColor.Green);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    statusmessage.Text = Line("Failed to switch App Name");
                    return;
                }*/
                /*
                string currentname = appname;
                appname = commandArguments.name;

                statusmessage.Text = ($"Deleting temporary app name");
                if (retryFunction(() => DeleteAppName(createdappinfo, currentname), 3))
                {
                    WriteLine("Success!", ConsoleColor.Green);
                }
                else
                {
                    WriteLine("Failed to delete Temporary App Name", ConsoleColor.Yellow);
                }
            }
                */
            
            statusmessage.Text = ($"Creating submission for {appname}");
            if (!retryFunction(() => CreateSubmission(createdappinfo), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            List<string> groupids = new List<string>();
            foreach (NeededGroupInfo groupinfo in Neededgroups)
            {
                groupids.Add(groupinfo.id);
            }

            statusmessage.Text = ($"Getting submission info for {appname}");
            List<NeededSubmissionInfo> returnedsubmissions = GetSubmissionInfo(createdappinfo);
            if (returnedsubmissions == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            NeededSubmissionInfo neededsubmissioninfo = returnedsubmissions[0];


            StoreInfo storeinfo = new StoreInfo()
            {
                ProductName = appname,
                BigId = createdappinfo.bigId,
                PublisherId = publisherinfo.sellerId,
                Visibility = new APPVisibility()
                {
                    GroupIds = appPublic.Checked ? new List<string>() : groupids,
                    DistributionMode = appPublic.Checked ? "Hidden": "Public",
                    Audience = appPublic.Checked ? "Public" : "PrivateBeta"
                }
            };

            statusmessage.Text = ($"Setting Pricing and Availibility for {appname}");
            if (!retryFunction(() => SetAvailibility(storeinfo, neededsubmissioninfo), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            statusmessage.Text = ($"Getting identity info for {appname}");

            Identity identity = GetIdentityInfo(createdappinfo);
            if (identity == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //create age rating application
            AgeRatingApplication ageratingapplication = new AgeRatingApplication() { product = new Product() { alias = appname } };
            statusmessage.Text = ($"Setting Age Ratings for {appname}");
            if (!retryFunction(() => SetAgeRatings(createdappinfo, neededsubmissioninfo, ageratingapplication), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }


            //get needed verification token
            statusmessage.Text = ("Getting request verifictation token");
            string requestverificationtoken = GetRequestToken(createdappinfo, neededsubmissioninfo);
            if (requestverificationtoken == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //set properties
            statusmessage.Text = ($"Setting properties for {appname}");
            if (!retryFunction(() => SetProperties(createdappinfo, neededsubmissioninfo, requestverificationtoken, !game.Checked), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            if (game.Checked)
            {
                //get xbl auth token
                statusmessage.Text = ($"Getting Xbox Live Auth Token for {appname}");
                ReturnedAuthInfo returnedauthinfo = GetXBLAuthToken(createdappinfo, "CERT CERT.DEBUG RETAIL ALL HIDDEN HISTORY");
                if (returnedauthinfo == null)
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                //get xblids
                statusmessage.Text = ($"Getting Xbox Live IDs for {appname}");
                GetXBLids(createdappinfo, HttpMethod.Options, null);
                XBLids xblids = GetXBLids(createdappinfo, HttpMethod.Get, returnedauthinfo);
                if (xblids == null)
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }
                
                //enable xbl for app
                statusmessage.Text = ($"Enabling Xbox Live for {appname}");
                EnableXBL(xblids, HttpMethod.Options, null, null);
                if (!retryFunction(() => EnableXBL(xblids, HttpMethod.Post, returnedauthinfo, appname), 3))
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                //get acc info
                statusmessage.Text = ($"Getting acc info");
                GetAccountInfo(xblids, HttpMethod.Options, null);
                XBLaccInfo xblaccinfo = GetAccountInfo(xblids, HttpMethod.Get, returnedauthinfo);
                if (xblaccinfo == null)
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                //get auth token for sandbox
                ReturnedAuthInfo returnedsandboxauthinfo = GetXBLAuthToken(createdappinfo, $"{xblaccinfo.OpenTierSandboxId} RETAIL");

                //send validation request
                statusmessage.Text = ($"Sending validation request");
                ValidateSandbox(xblids, HttpMethod.Options, null, xblaccinfo);
                string sourceversion = ValidateSandbox(xblids, HttpMethod.Post, returnedsandboxauthinfo, xblaccinfo);
                if (sourceversion == null)
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                //send copy request
                statusmessage.Text = ($"Copying {xblaccinfo.OpenTierSandboxId} SANDBOX to RETAIL");
                CopySandbox(xblids, HttpMethod.Options, null, xblaccinfo);
                if (!retryFunction(() => CopySandbox(xblids, HttpMethod.Post, returnedsandboxauthinfo, xblaccinfo), 3))
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

                //send publish request
                statusmessage.Text = ($"Publishing Sandbox");
                PublishSandbox(xblids, HttpMethod.Options, null, xblaccinfo, sourceversion);
                if (!retryFunction(() => PublishSandbox(xblids, HttpMethod.Post, returnedsandboxauthinfo, xblaccinfo, sourceversion), 3))
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }
            }
            //patch the package
            if (true)
            {
                bool uploadfile = false;
                statusmessage.Text = ($"Starting to patch {filename}");
                FileInfo fileInfo = new FileInfo(filepath);
                if ((fileInfo.Extension == ".appxbundle") || (fileInfo.Extension == ".msixbundle"))
                {

                    WriteLine("Detected Bundle Package", ConsoleColor.Green);
                    statusmessage.Text = ($"Extracting {filename}");
                    string bundlepath = Path.Join(Path.GetTempPath(), "bundle");
                    if (Directory.Exists(bundlepath))
                    {
                        Directory.Delete(bundlepath, true);
                    }
                    ZipFile.ExtractToDirectory(fileInfo.FullName, bundlepath, true);
                    XDocument AppxBundleManifest = XDocument.Load(Path.Join(bundlepath, "AppxMetadata\\AppxBundleManifest.xml"));
                    XNamespace ns = AppxBundleManifest.Root.GetDefaultNamespace();
                    XElement packages = AppxBundleManifest.Root.Element(XName.Get("Packages", ns.NamespaceName));
                    if (packages != null)
                    {
                        foreach (XElement package in packages.Elements())
                        {
                            if ((package.Attribute("Type").Value == "application"))
                            {
                                if (package.Attribute("Architecture").Value == "x64")
                                {
                                    filepath = Path.Join(bundlepath, HttpUtility.UrlPathEncode(package.Attribute("FileName").Value));
                                    filename = HttpUtility.UrlPathEncode(package.Attribute("FileName").Value);
                                }
                            }
                        }
                    }
                    else
                    {
                        WriteLine("Failed to find packages from package bundle", ConsoleColor.Red);
                        return null;
                    }
                }
                else if (
                    !(fileInfo.Extension == ".appx") || 
                    !(fileInfo.Extension == ".msix") ||
                    !(fileInfo.Extension == ".appxupload") || !
                    (fileInfo.Extension == ".msixupload"))
                {
                    WriteLine("Unknown filetype", ConsoleColor.Red);
                    return null;
                }


                if (!uploadfile)
                {
                    //patch appxmanifest
                    statusmessage.Text = ($"Patching appxmanifest for {filename}");
                    string packagepath = Path.Join(Path.GetTempPath(), "package", $"{System.IO.Path.GetFileNameWithoutExtension(filename)}");
                    //clear out package path too
                    if (Directory.Exists(packagepath))
                    {
                        Directory.Delete(packagepath, true);
                    }
                    ZipFile.ExtractToDirectory(filepath, packagepath, true);
                    XDocument AppxManifest = XDocument.Load(Path.Join(packagepath, "AppxManifest.xml"));
                    string defaultnamespace = AppxManifest.Root.GetDefaultNamespace().NamespaceName;
                    XElement identityelement = AppxManifest.Root.Element(XName.Get("Identity", defaultnamespace));
                    identityelement.Attribute("Name").Value = identity.Name;
                    identityelement.Attribute("Publisher").Value = identity.Publisher;
                    identityelement.Attribute("ProcessorArchitecture").Value = "x64";
                    string[] Version = identityelement.Attribute("Version").Value.Split(".");
                    Version[3] = "0";
                    identityelement.Attribute("Version").Value = string.Join(".", Version);
                    XElement displayname = AppxManifest.Root.Element(XName.Get("Properties", defaultnamespace)).Element(XName.Get("DisplayName", defaultnamespace));
                    displayname.Value = appname;
                    XElement publisherdisplayname = AppxManifest.Root.Element(XName.Get("Properties", defaultnamespace)).Element(XName.Get("PublisherDisplayName", defaultnamespace));
                    publisherdisplayname.Value = identity.PublisherDisplayName;
                    AppxManifest.Save(Path.Join(packagepath, "AppxManifest.xml"));
                    statusmessage.Text = ($"Generating patched {filename}");
                    //if we are on windows and the exe or if we aren't and the binary can't be found
                    if ((!File.Exists(Path.Join(Directory.GetCurrentDirectory(), "MakeMsix", "MakeMsix.exe")) && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) || (!File.Exists(Path.Join(Directory.GetCurrentDirectory(), "makemsix", "makemsix")) && !System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)))
                    {
                        WriteLine("MakeMsix was not found", ConsoleColor.Red);
                        return null;
                    }

                    string tempfilename = $"{RandomString(10)}.appx";
                    string appxpath = Path.Join(Path.GetTempPath(), tempfilename);
                    bool msixcreation = retryFunction(() => MakeMsix(packagepath, appxpath), 3);
                    //clear out package path too
                    if (Directory.Exists(packagepath))
                    {
                        Directory.Delete(packagepath, true);
                    }
                    if (msixcreation)
                    {
                        filepath = appxpath;
                        filename = tempfilename;
                    }
                    else
                    {
                        WriteLine("Failed", ConsoleColor.Red);
                        return null;
                    }
                    statusmessage.Text = ($"Generating appxsym for {filename}");
                    string appxsymfilename = $"{Path.GetFileNameWithoutExtension(filepath)}.appxsym";
                    string appxsympath = Path.Join(Path.GetTempPath(), appxsymfilename);
                    if (File.Exists(appxsympath))
                    {
                        File.Delete(appxsympath);
                    }
                    if (Directory.Exists(appxsympath))
                    {
                        Directory.Delete(appxsympath, true);
                    }
                    //create sym zip file then add a dummy pdb lol
                    using (var zip = ZipFile.Open(appxsympath, ZipArchiveMode.Create))
                    {
                        //create entry for file
                        var entry = zip.CreateEntry("test.pdb");
                        //set last write time to the current time
                        entry.LastWriteTime = DateTimeOffset.Now;
                        //open the file as a stream
                        using (var stream = GenerateStreamFromString(""))
                        using (var entryStream = entry.Open()) // copy the stream into the entry
                            stream.CopyTo(entryStream);
                    }
                    statusmessage.Text = ($"Generating appxupload from {filename}");
                    string appxuploadfilename = $"{Path.GetFileNameWithoutExtension(filepath)}.appxupload";
                    string appxuploadpath = Path.Join(Path.GetTempPath(), appxuploadfilename);
                    if (File.Exists(appxuploadpath))
                    {
                        File.Delete(appxuploadpath);
                    }
                    if (Directory.Exists(appxuploadpath))
                    {
                        Directory.Delete(appxuploadpath, true);
                    }
                    //create upload zip file
                    using (var zip = ZipFile.Open(appxuploadpath, ZipArchiveMode.Create))
                    {
                        //create entry for file
                        var entry = zip.CreateEntry(filename);
                        //set last write time to the current time
                        entry.LastWriteTime = DateTimeOffset.Now;
                        //open the file as a stream
                        using (var stream = File.OpenRead(filepath))
                        using (var entryStream = entry.Open()) // copy the stream into the entry
                            stream.CopyTo(entryStream);
                        //create entry for appxsym
                        entry = zip.CreateEntry(appxsymfilename);
                        //set last write time to the current time
                        entry.LastWriteTime = DateTimeOffset.Now;
                        //open the file as a stream
                        using (var stream = File.OpenRead(appxsympath))
                        using (var entryStream = entry.Open()) // copy the stream into the entry
                            stream.CopyTo(entryStream);
                    }
                    filepath = appxuploadpath;
                    filename = appxuploadfilename;

                }
            }
            statusmessage.Text = ($"Getting upload info for {appname}");
            CreateUploadInfo createuploadinfo = new CreateUploadInfo() { FileName = filename };
            NeededUploadInfo neededuploadinfo = GetUploadInfo(createuploadinfo, neededsubmissioninfo);
            if (neededuploadinfo == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
            }

            string token = HttpUtility.ParseQueryString(new Uri(neededuploadinfo.UploadInfo.SasUrl).Query).Get("token");

            statusmessage.Text = ($"Setting Metadata for {appname}");

            //read package as a filestream
            FileStream packagestream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

            //set the metadata for the file this has to be done here as unfortunately there are too many variables to pass, it is bad practice I know but its just too much to put into a function
            string longurl = $"https://upload.xboxlive.com/upload/setmetadata/{neededuploadinfo.UploadInfo.XfusId}";
            var uriBuilder = new UriBuilder(longurl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["filename"] = filename;
            query["fileSize"] = packagestream.Length.ToString();
            query["token"] = token;
            uriBuilder.Query = query.ToString();
            longurl = uriBuilder.ToString();

            var request = new HttpRequestMessage(HttpMethod.Options, longurl);
            var response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            request = new HttpRequestMessage(HttpMethod.Post, longurl);
            response = client.SendAsync(request);
            response.Wait();

            NeededMetadata neededmetadata;
            if (!response.Result.IsSuccessStatusCode)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            string responseresult = response.Result.Content.ReadAsStringAsync().Result;
            neededmetadata = JsonConvert.DeserializeObject<NeededMetadata>(responseresult);

            //chunksize in bytes
            int chunksize = Convert.ToInt32(neededmetadata.ChunkSize);
            //number of chunks
            decimal chunksnum = packagestream.Length / chunksize;
            int chunks = Convert.ToInt32(Math.Ceiling(chunksnum)) + 1;


            for (int i = 0; i < chunks; i++)
            {
                int chunknum = i + 1;
                statusmessage.Text = ($"Setting metadata for chunk number {chunknum} of {chunks}");
                if (!UploadChunk(token, chunknum, null, neededuploadinfo, HttpMethod.Options))
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }

            }

            for (int i = 0; i < chunks; i++)
            {
                long position = (i * (long)chunksize);
                int toRead = (int)Math.Min(packagestream.Length - position + 1, chunksize);
                byte[] buffer = new byte[toRead];
                packagestream.Read(buffer, 0, buffer.Length);
                int chunknum = i + 1;
                statusmessage.Text = ($"Uploading chunk number {chunknum} of {chunks}");
                if (!retryFunction(() => UploadChunk(token, chunknum, buffer, neededuploadinfo, HttpMethod.Post), 3))
                {
                    WriteLine("Failed", ConsoleColor.Red);
                    return null;
                }
            }

            statusmessage.Text = ($"Marking upload as finished");

            if (!retryFunction(() => UploadFinished(token, neededuploadinfo, HttpMethod.Options), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            if (!retryFunction(() => UploadFinished(token, neededuploadinfo, HttpMethod.Post), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            
            CommitalInfo commitalinfo = new CommitalInfo() { Id = neededuploadinfo.Id };

            statusmessage.Text = ($"Commiting upload");
            if (!retryFunction(() => CommitUpload(commitalinfo), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            statusmessage.Text = ($"Set target platforms");

            if (!retryFunction(() => SetPlatforms(neededsubmissioninfo), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //set languages
            statusmessage.Text = ($"Getting manage languages request verification token");
            //get needed verification token
            string managelanguagerequestverificationtoken = GetManageLanguagesRequestToken(createdappinfo, neededsubmissioninfo);
            if (managelanguagerequestverificationtoken == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            statusmessage.Text = ($"Setting languages for {appname}");
            if (!SetLanguages(createdappinfo, neededsubmissioninfo, managelanguagerequestverificationtoken))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //get listing info
            statusmessage.Text = ($"Getting listing info {appname}");

            //get needed listing info
            ListingInfo listinginfo = GetListingInfo(createdappinfo, neededsubmissioninfo);
            if (listinginfo == null)
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //set listing
            statusmessage.Text = ($"Setting languages for {appname}");

            if (!retryFunction(() => SetListing(createdappinfo, neededsubmissioninfo, listinginfo, appname, appdescription), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //upload screenshot
            statusmessage.Text = ($"Adding screenshot for {appname}");
            if (!retryFunction(() => UploadScreenShot(neededsubmissioninfo, createdappinfo, HttpMethod.Post, listinginfo, appscreenshotname), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }

            //push to store
            statusmessage.Text = ($"submitting {appname} to store");
            if (!retryFunction(() => SubmitToStore(neededsubmissioninfo, createdappinfo), 3))
            {
                WriteLine("Failed", ConsoleColor.Red);
                return null;
            }


            statusmessage.Text = ("SUCCESS!!!");

            MessageBox.Show("You can observe the progress here " + 
                $"https://partner.microsoft.com/en-us/dashboard/products/{createdappinfo.bigId}/submissions/{neededsubmissioninfo.id}/CertificationStatus\n" +
                "Once the app passes certification you can use the following deeplink to install the app: " +
                $"ms-windows-store://pdp/?productid={createdappinfo.bigId}");

            linkLabel1.Text = "App Certification Status";
            linkLabel1.LinkVisited = false;
            StorePage = $"https://partner.microsoft.com/en-us/dashboard/products/{createdappinfo.bigId}/submissions/{neededsubmissioninfo.id}/CertificationStatus";
            return createdappinfo.bigId;
        }



        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomInvisibleString(int length)
        {
            const string chars = "​‍؜​⁪⁫⁬";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        static string GetRequestToken(NeededAppInfo createdappinfo, NeededSubmissionInfo neededsubmissioninfo)
        {
            Thread.Sleep(5000);
            string url = (partneruri.ToString() + $"en-us/dashboard/products/{createdappinfo.bigId}/submissions/{neededsubmissioninfo.id}/properties");
            var response = client.GetAsync(url);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();
                string responseresult = result.Result;
                HtmlAgilityPack.HtmlDocument pageDocument = new HtmlAgilityPack.HtmlDocument();
                pageDocument.LoadHtml(responseresult);
                var node = pageDocument.DocumentNode.SelectSingleNode("//*[@name=\"__RequestVerificationToken\"]");
                string token = node.Attributes["Value"].Value;
                return token;
            }
            else
            {
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();
                string responseresult = result.Result;
                return null;
            }
        }

        static string GetManageLanguagesRequestToken(NeededAppInfo createdappinfo, NeededSubmissionInfo neededsubmissioninfo)
        {
            //this thread .sleep may not be needed but I don't want to remove it just in case
            Thread.Sleep(1000);
            string url = (partneruri.ToString() + $"en-us/dashboard/products/{createdappinfo.bigId}/submissions/{neededsubmissioninfo.id}/ManageLanguages?ProductType=App");
            var response = client.GetAsync(url);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();
                string responseresult = result.Result;
                HtmlAgilityPack.HtmlDocument pageDocument = new HtmlAgilityPack.HtmlDocument();
                pageDocument.LoadHtml(responseresult);
                var node = pageDocument.DocumentNode.SelectSingleNode("//*[@name=\"__RequestVerificationToken\"]");
                string token = node.Attributes["Value"].Value;
                return token;
            }
            else
            {
                return null;
            }
        }

        static ListingInfo GetListingInfo(NeededAppInfo createdappinfo, NeededSubmissionInfo neededsubmissioninfo)
        {
            //thread.sleep probably not needed
            Thread.Sleep(1000);
            string url = (partneruri.ToString() + $"en-us/dashboard/products/{createdappinfo.bigId}/submissions/{neededsubmissioninfo.id}/listings?languageId=4");
            var response = client.GetAsync(url);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();
                string responseresult = result.Result;
                HtmlAgilityPack.HtmlDocument pageDocument = new HtmlAgilityPack.HtmlDocument();
                pageDocument.LoadHtml(responseresult);
                ListingInfo listinginfo = new ListingInfo();
                var node = pageDocument.DocumentNode.SelectSingleNode("//*[@name=\"__RequestVerificationToken\"]");
                listinginfo.RequestVerificationToken = node.Attributes["Value"].Value;
                var nodetwo = pageDocument.DocumentNode.SelectSingleNode("//*[@name=\"CommonListing.Id\"]");
                listinginfo.ListingId = nodetwo.Attributes["Value"].Value;
                return listinginfo;
            }
            else
            {
                return null;
            }
        }

        static bool SetProperties(NeededAppInfo createdappinfo, NeededSubmissionInfo neededsubmissioninfo, string verificationtoken, bool app)
        {
            string url = (partneruri.ToString() + $"en-us/dashboard/listings/properties/Properties?appId={createdappinfo.bigId}&submissionId={neededsubmissioninfo.id}");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            MultipartFormDataContent form;
            if (!app)
            {
                form = new MultipartFormDataContent(("------WebKitFormBoundary" + RandomString(16)));
                HttpContent tempcontent;
                tempcontent = new StringContent(verificationtoken);
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "__RequestVerificationToken" };
                form.Add(tempcontent);
                tempcontent = new StringContent("0");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Id" };
                form.Add(tempcontent);
                tempcontent = new StringContent("False");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsAddOn" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AddOnId" };
                form.Add(tempcontent);
                tempcontent = new StringContent("0");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AddOnSubmissionId" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AreGamingOptionsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsUrlFieldMigrated" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Games");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "Category" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[0].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[0].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ActionAndAdventure");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[0].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[1].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("CardAndboard");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[1].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[2].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Casino");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[2].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[3].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Classics");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[3].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[4].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Educational");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[4].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[5].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("FamilyAndKids");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[5].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[6].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Fighting");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[6].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[7].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("MultiPlayerOnlineBattleArena");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[7].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[8].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("GamesMusic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[8].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[9].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Other");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[9].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[10].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Platformer");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[10].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[11].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("PuzzleAndTrivia");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[11].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[12].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("RacingAndFlying");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[12].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[13].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("RolePlaying");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[13].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[14].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Shooter");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[14].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[15].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Simulation");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[15].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[16].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("GamesSports");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[16].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[17].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Strategy");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[17].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[18].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Tools");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[18].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[19].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Word");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[19].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Yes");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "PrivatePolicyRequiredState" };
                form.Add(tempcontent);
                tempcontent = new StringContent("test.com");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "PrivatePolicyUrl" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "WebsiteUrl" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "SupportContact" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SinglePlayer.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SinglePlayer.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SharedSplitScreen.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SharedSplitScreen.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsLocalMultiplayer" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsOnlineMultiplayer" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsCrossPlayEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsLocalCooperative" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsOnlineCooperative" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.CrossPlatformCoop.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsBroadcastingPrivilegeGranted" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsBroadcastingPrivilegeGranted" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Resolution4k.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.HighDynamicRange.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.HighDynamicRange.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.VariableRefreshRate.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.MixedReality.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.MixedReality.IsHolographicSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "Has3rdPartyIAP" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsAccessible" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsRemovableMediaEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsRemovableMediaEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsBackupRestoreEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsBackupRestoreEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsGameDvrEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsGameDvrEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CanSendKinectDataToExternal" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Continuum.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.PenInk.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Cortana.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tch");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tch");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("kbd");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("kbd");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mse");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mse");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("cmr");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("cmr");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("hce");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("hce");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("nfc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("nfc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ble");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ble");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tel");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tel");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("xgp");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("xgp");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mct");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mct");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mrc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mrc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.RamMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.RamRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.DirectXMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.DirectXRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.VideoRamMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.VideoRamRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.ProcessorMinText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.ProcessorRecText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.GraphicsMinText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.GraphicsRecText" };
                form.Add(tempcontent);
            }
            else
            {
                form = new MultipartFormDataContent(("----WebKitFormBoundary" + RandomString(16)));
                HttpContent tempcontent;
                tempcontent = new StringContent(verificationtoken);
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "__RequestVerificationToken" };
                form.Add(tempcontent);
                tempcontent = new StringContent("0");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Id" };
                form.Add(tempcontent);
                tempcontent = new StringContent("False");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsAddOn" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AddOnId" };
                form.Add(tempcontent);
                tempcontent = new StringContent("0");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AddOnSubmissionId" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AreGamingOptionsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsUrlFieldMigrated" };
                form.Add(tempcontent);
                tempcontent = new StringContent("FoodAndDining");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "Category" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[0].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ActionAndAdventure");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[0].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[1].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("CardAndboard");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[1].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[2].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Casino");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[2].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[3].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Classics");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[3].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[4].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Educational");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[4].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[5].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("FamilyAndKids");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[5].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[6].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Fighting");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[6].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[7].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("MultiPlayerOnlineBattleArena");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[7].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[8].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("GamesMusic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[8].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[9].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Other");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[9].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[10].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Platformer");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[10].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[11].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("PuzzleAndTrivia");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[11].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[12].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("RacingAndFlying");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[12].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[13].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("RolePlaying");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[13].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[14].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Shooter");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[14].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[15].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Simulation");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[15].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[16].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("GamesSports");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[16].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[17].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Strategy");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[17].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[18].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Tools");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[18].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[19].Selected" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Word");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "AllGenres[19].Genre" };
                form.Add(tempcontent);
                tempcontent = new StringContent("Yes");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "PrivatePolicyRequiredState" };
                form.Add(tempcontent);
                tempcontent = new StringContent("test.com");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "PrivatePolicyUrl" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "WebsiteUrl" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "SupportContact" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SinglePlayer.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SinglePlayer.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SharedSplitScreen.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.SharedSplitScreen.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsLocalMultiplayer" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsOnlineMultiplayer" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsCrossPlayEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsLocalCooperative" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsOnlineCooperative" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.CrossPlatformCoop.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsBroadcastingPrivilegeGranted" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.IsBroadcastingPrivilegeGranted" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Resolution4k.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.HighDynamicRange.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.HighDynamicRange.IsXboxSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.VariableRefreshRate.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.MixedReality.IsDesktopSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.MixedReality.IsHolographicSupported" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "Has3rdPartyIAP" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsAccessible" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsRemovableMediaEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsRemovableMediaEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsBackupRestoreEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsBackupRestoreEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("true");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsGameDvrEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "IsGameDvrEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CanSendKinectDataToExternal" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Continuum.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.PenInk.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "GamingOptions.Cortana.IsEnabled" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tch");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[0].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tch");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[0].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("kbd");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[1].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("kbd");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[1].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mse");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[2].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mse");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[2].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("cmr");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[3].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("cmr");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[3].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("hce");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[4].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("hce");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[4].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("nfc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[5].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("nfc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[5].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ble");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[6].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("ble");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[6].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tel");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[7].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("tel");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[7].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[8].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mic");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[8].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("xgp");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[9].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("xgp");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[9].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mct");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[10].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mct");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[10].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mrc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareMinimum[11].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("false");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].IsChecked" };
                form.Add(tempcontent);
                tempcontent = new StringContent("mrc");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].HardwareItem.ShortCode" };
                form.Add(tempcontent);
                tempcontent = new StringContent("True");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.HardwareRecommended[11].IsPermissionSatisfied" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.RamMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.RamRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.DirectXMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.DirectXRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.VideoRamMinSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.VideoRamRecSelection" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.ProcessorMinText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.ProcessorRecText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.GraphicsMinText" };
                form.Add(tempcontent);
                tempcontent = new StringContent("");
                tempcontent.Headers.Clear();
                tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "DetailedHardwareViewModel.GraphicsRecText" };
                form.Add(tempcontent);
            }

            request.Content = form;

            var response = client.SendAsync(request);
            response.Wait();
            int statuscode = (int)response.Result.StatusCode;
            if (statuscode < 400)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool SetListing(NeededAppInfo createdappinfo, NeededSubmissionInfo neededsubmissioninfo, ListingInfo listinginfo, string name, string description)
        {
            MultipartFormDataContent form = new MultipartFormDataContent(("------WebKitFormBoundary" + RandomString(16)));
            HttpContent tempcontent;
            tempcontent = new StringContent(listinginfo.ListingId);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "custom-listing-cloneprimary" };
            form.Add(tempcontent);
            tempcontent = new StringContent(listinginfo.RequestVerificationToken);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "__RequestVerificationToken" };
            form.Add(tempcontent);
            tempcontent = new StringContent("0");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.ApplicationId" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.InAppPurchaseId" };
            form.Add(tempcontent);
            tempcontent = new StringContent("0");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.LanguageId" };
            form.Add(tempcontent);
            tempcontent = new StringContent(neededsubmissioninfo.id);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.SubmissionId" };
            form.Add(tempcontent);
            tempcontent = new StringContent(listinginfo.ListingId);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Id" };
            form.Add(tempcontent);
            tempcontent = new StringContent("App");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ProductType" };
            form.Add(tempcontent);
            tempcontent = new StringContent(listinginfo.ListingId);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Id" };
            form.Add(tempcontent);
            tempcontent = new StringContent("4");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.LanguageId" };
            form.Add(tempcontent);
            tempcontent = new StringContent(name);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Title" };
            form.Add(tempcontent);
            tempcontent = new StringContent(description);
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Description" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.ReleaseNotes" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[0]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[1]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[2]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[3]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[4]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[5]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[6]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[7]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[8]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[9]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[10]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[11]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[12]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[13]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[14]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[15]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[16]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[17]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[18]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.Features[19]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("1");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "visibileFeatures-1152922700006424301" };
            form.Add(tempcontent);
            tempcontent = new StringContent("false");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.OverridePackageLogos" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HeroTrailerId" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.ShortTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.ShortTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.SortTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.SortTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.VoiceTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.VoiceTitle" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.ShortDescription" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.ShortDescription" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[0]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[1]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[2]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[3]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[4]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[5]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[6]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[7]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[8]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[9]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.MinimumHardwareNotesList[10]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("1");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.visibileHardware-min" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[0]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[1]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[2]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[3]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[4]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[5]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[6]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[7]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[8]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[9]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.RecommendedHardwareNotesList[10]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("1");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.HardwareNotes.visibileHardware-rec" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.SupportContact" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[0]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[1]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[2]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[3]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[4]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[5]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Keywords[6]" };
            form.Add(tempcontent);
            tempcontent = new StringContent("1");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "visibleKeywords" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.Trademark" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "CommonListing.LicenseTerm" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.DevStudio" };
            form.Add(tempcontent);
            tempcontent = new StringContent("");
            tempcontent.Headers.Clear();
            tempcontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "ListingModels[0].Listing.DevStudio" };
            form.Add(tempcontent);

            string url = (partneruri.ToString() + $"en-us/dashboard/listings/Listing/Listings?appId={createdappinfo.bigId}&submissionId={neededsubmissioninfo.id}&languageId=4&listingId={listinginfo.ListingId}");
            var response = client.PostAsync(url, form);
            response.Wait();
            int statuscode = (int)response.Result.StatusCode;
            if (statuscode < 400)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static XBLids GetXBLids(NeededAppInfo neededappinfo, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo)
        {
            string url = xbluri.ToString() + $"products?alternateId={neededappinfo.bigId}";
            var request = new HttpRequestMessage(httpmethod, url);
            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
            }
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<XBLids>>(responseresult)[0];
            }
            else
            {
                return null;
            }
        }
        static bool EnableXBL(XBLids xblids, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo, string name)
        {
            string url = xbluri.ToString() + $"products/{xblids.ProductId}/enable";
            var request = new HttpRequestMessage(httpmethod, url);
            request.Headers.Add("Origin", partneruri.ToString());
            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                EnableClass enableclass = new EnableClass() { LocalizedTitleNames = new List<LocalizedTitleName>() { new LocalizedTitleName() { Value = $"{name} - Game" } } };
                var content = JsonConvert.SerializeObject(enableclass);
                request.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            }
            else
            {
                request.Headers.Add("Access-Control-Request-Method", "POST");
            }

            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static XBLaccInfo GetAccountInfo(XBLids xblids, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo)
        {
            string url = xbluri.ToString() + $"accounts/{xblids.AccountId}";
            var request = new HttpRequestMessage(httpmethod, url);
            request.Headers.Add("Origin", partneruri.ToString());
            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
            }
            else
            {
                request.Headers.Add("Access-Control-Request-Method", "GET");
            }

            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<XBLaccInfo>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static bool SubmitToStore(NeededSubmissionInfo neededsubmissioninfo, NeededAppInfo neededappinfo)
        {
            string url = partneruri.ToString() + $"en-us/dashboard/product/api/{neededappinfo.bigId}/submissions/{neededsubmissioninfo.id}/submit";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(new SubmitInfo()), System.Text.Encoding.UTF8, "application/json");
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static string ValidateSandbox(XBLids xblids, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo, XBLaccInfo xblaccinfo)
        {
            string url = xbluri.ToString() + $"products/{xblids.ProductId}/sandboxes/{xblaccinfo.OpenTierSandboxId}/publish?option=Validate";
            var request = new HttpRequestMessage(httpmethod, url);

            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            }
            else
            {
                request.Headers.Add("Access-Control-Request-Method", "POST");
            }

            request.Headers.Add("Origin", partneruri.ToString());
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                if (responseresult != "")
                {
                    TempValidateClass responseobject = JsonConvert.DeserializeObject<TempValidateClass>(responseresult);
                    string status = responseobject.Status;
                    while (status != "Success")
                    {
                        Thread.Sleep(5000);
                        request = new HttpRequestMessage(httpmethod, url);
                        request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                        response = client.SendAsync(request);
                        response.Wait();
                        responseresult = response.Result.Content.ReadAsStringAsync().Result;
                        responseobject = JsonConvert.DeserializeObject<TempValidateClass>(responseresult);
                        if (responseresult != null)
                            status = responseobject.Status;
                    }
                    return responseobject.SourceVersion;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        static bool CopySandbox(XBLids xblids, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo, XBLaccInfo xblaccinfo)
        {
            string url = xbluri.ToString() + $"products/{xblids.ProductId}/sandboxes/RETAIL/sources/{xblaccinfo.OpenTierSandboxId}/copy";
            var request = new HttpRequestMessage(httpmethod, url);

            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            }
            else
            {
                request.Headers.Add("Access-Control-Request-Method", "POST");
            }

            request.Headers.Add("Origin", partneruri.ToString());
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                if (responseresult != "" && httpmethod == HttpMethod.Post)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        static bool PublishSandbox(XBLids xblids, HttpMethod httpmethod, ReturnedAuthInfo returnedauthinfo, XBLaccInfo xblaccinfo, string sourceversion)
        {
            string url = xbluri.ToString() + $"products/{xblids.ProductId}/sandboxes/{xblaccinfo.OpenTierSandboxId}/publish";
            var request = new HttpRequestMessage(httpmethod, $"{url}?option=Publish&version={sourceversion}");

            if (returnedauthinfo != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            }
            else
            {
                request.Headers.Add("Access-Control-Request-Method", "POST");
            }

            request.Headers.Add("Origin", partneruri.ToString());
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode && httpmethod == HttpMethod.Post)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                if (responseresult != "")
                {
                    TempValidateClass responseobject = JsonConvert.DeserializeObject<TempValidateClass>(responseresult);
                    string status = responseobject.Status;
                    while (status != "Success")
                    {
                        Thread.Sleep(2000);
                        request = new HttpRequestMessage(HttpMethod.Get, url);
                        request.Headers.Authorization = new AuthenticationHeaderValue($"XBL3.0", $"x=-;{returnedauthinfo.Token}");
                        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                        response = client.SendAsync(request);
                        response.Wait();
                        responseresult = response.Result.Content.ReadAsStringAsync().Result;
                        responseobject = JsonConvert.DeserializeObject<TempValidateClass>(responseresult);
                        status = responseobject.Status;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        static ReturnedAuthInfo GetXBLAuthToken(NeededAppInfo neededappinfo, string sandboxes)
        {
            string url = partneruri.ToString() + $"xdts/authorize";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonConvert.SerializeObject(new XBLAuthInfo() { Properties = new PropertiesClass() { DseAppId = neededappinfo.bigId, Sandboxes = sandboxes } }));
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<ReturnedAuthInfo>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static PublisherInfo GetPublisherInfo()
        {
            var response = client.GetAsync((partneruri.ToString() + $"/en-us/dashboard/account/v3/api/accessmanagement/userinfobypuid"));
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<PublisherInfo>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static List<NeededGroupInfo> GetAllGroups()
        {
            var response = client.GetAsync((partneruri.ToString() + "/dashboard/monetization/group-management/api/groups"));
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<NeededGroupInfo>>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static NeededAppInfo CreateApp(AppInfo appinfo)
        {
            var stringPayload = JsonConvert.SerializeObject(appinfo);
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync((partneruri.ToString() + "/en-US/dashboard/product/api/products"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<NeededAppInfo>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static bool CreateSubmission(NeededAppInfo appinfo)
        {
            var stringPayload = "{ \"publishingDetailsVisible\":false}";
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync((partneruri.ToString() + $"/en-us/dashboard/product/api/{appinfo.id}/submissions"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(responseresult))
                    return false;
                //return JsonConvert.DeserializeObject<NeededAppInfo>(responseresult);
                return true;
            }
            else
            {
                return false;
                // return null;
            }
        }

        static List<NeededSubmissionInfo> GetSubmissionInfo(NeededAppInfo createdappinfo)
        {
            var response = client.GetAsync((partneruri.ToString() + $"/en-us/dashboard/product/api/{createdappinfo.id}/submissions"));
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<NeededSubmissionInfo>>(responseresult);
            }
            else
            {
                return null;
            }
        }



        static bool SetAvailibility(StoreInfo storeinfo, NeededSubmissionInfo neededsubmissioninfo)
        {
            var stringPayload = JsonConvert.SerializeObject(storeinfo);
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync((partneruri.ToString() + $"/en-us/dashboard/availability/api/product/{storeinfo.BigId}/submissions/{neededsubmissioninfo.id}"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool SetAgeRatings(NeededAppInfo neededAppInfo, NeededSubmissionInfo neededsubmissioninfo, AgeRatingApplication ageratingapplication)
        {
            var stringPayload = JsonConvert.SerializeObject(ageratingapplication);
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PutAsync((partneruri.ToString() + $"/en-US/dashboard/ageratings/api/products/{neededAppInfo.bigId}/settings?submissionId={neededsubmissioninfo.id}"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool SetLanguages(NeededAppInfo neededAppInfo, NeededSubmissionInfo neededsubmissioninfo, string requestverificationtoken)
        {
            string url = partneruri.ToString() + $"/en-us/dashboard/listings/managelanguages/ManageAppLanguages";
            Languages languages = new Languages() { AppId = neededAppInfo.bigId, SubId = neededsubmissioninfo.id };
            var stringPayload = JsonConvert.SerializeObject(languages);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            request.Headers.Add("__RequestVerificationToken", requestverificationtoken);
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static NeededUploadInfo GetUploadInfo(CreateUploadInfo createuploadinfo, NeededSubmissionInfo neededsubmissioninfo)
        {
            var stringPayload = JsonConvert.SerializeObject(createuploadinfo);
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync((partneruri.ToString() + $"/dashboard/packages/api/pkg/v2.0/packages?packageSetId=PS-{neededsubmissioninfo.id}&storage=xfus"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<NeededUploadInfo>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static bool SetPlatforms(NeededSubmissionInfo neededsubmissioninfo)
        {
            var stringPayload = JsonConvert.SerializeObject(new TargetPlatforms());
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, (partneruri.ToString() + $"/dashboard/packages/api/pkg/v2.0/packagesets/PS-{neededsubmissioninfo.id}?groupId=Base"));
            request.Content = content;
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool UploadChunk(string token, int chunknum, byte[] chunk, NeededUploadInfo neededuploadinfo, HttpMethod httpmethod)
        {
            var uriBuilder = new UriBuilder((xboxuri + $"/upload/uploadchunk/{neededuploadinfo.UploadInfo.XfusId}"));
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["blockNumber"] = chunknum.ToString();
            query["runUploadSynchronously"] = true.ToString();
            query["token"] = token;
            uriBuilder.Query = query.ToString();
            string uristring = uriBuilder.ToString();
            var request = new HttpRequestMessage(httpmethod, uristring);
            if (chunk != null)
            {
                request.Content = new ByteArrayContent(chunk);
            }
            var response = client.SendAsync(request);
            try
            {
                response.Wait();
            }
            catch
            {
                return false;
            }
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool UploadScreenShot(NeededSubmissionInfo neededsubmissioninfo, NeededAppInfo neededappinfo, HttpMethod httpmethod, ListingInfo listinginfo, string image)
        {
            //file for screenshot is currently hardcoded may change this at somepoint but this is currently not a priority
            FileStream screenshot = new FileStream(image, FileMode.Open, FileAccess.Read);
            string partnercenterurl = partneruri + $"en-us/dashboard/listings/FileUploadV2/CreateContext?parentId={neededappinfo.bigId}&parentType=0";
            var request = new HttpRequestMessage(HttpMethod.Post, partnercenterurl);
            ScreenshotInfo screenshotinfo = new ScreenshotInfo() { };
            var stringPayload = JsonConvert.SerializeObject(screenshotinfo);
            request.Content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
                return false;

            string responseresult = response.Result.Content.ReadAsStringAsync().Result;
            ScreenshotResponse screenshotresponse = JsonConvert.DeserializeObject<ScreenshotResponse>(responseresult);

            //update request
            string updateurl = partneruri + $"en-us/dashboard/listings/FileUploadV2/UpdateStatus?parentId={neededappinfo.bigId}&parentType=0&id={screenshotresponse.FileId}";
            request = new HttpRequestMessage(HttpMethod.Post, updateurl);
            stringPayload = JsonConvert.SerializeObject(new UpdateStatus());
            request.Content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
                return false;

            string listingasseturl = partneruri + $"en-us/dashboard/listings/Listing/ListingAsset?productId={neededappinfo.bigId}&submissionId={neededsubmissioninfo.id}&listingId={listinginfo.ListingId}&listingAssetId=0&productType=App&languageId=4";
            request = new HttpRequestMessage(HttpMethod.Post, listingasseturl);
            stringPayload = $"__RequestVerificationToken={listinginfo.RequestVerificationToken}&FileId={screenshotresponse.FileId}&AssetType=Screenshot&DisplayOrder=0&FileName={"blank.png"}&FileSize={screenshot.Length}";
            request.Content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
                return false;

            //do some block stuff, idk what the f*** this does or why it needs to be done
            string ingestblocklisturl = $"{screenshotresponse.UploadSasUrl}&comp=blocklist";
            request = new HttpRequestMessage(HttpMethod.Options, ingestblocklisturl);
            request.Headers.Add("Access-Control-Request-Method", "GET");
            request.Headers.Add("Access-Control-Request-Headers", "x-ms-blob-type,x-ms-date,x-version");
            request.Headers.Add("Origin", "https://partner.microsoft.com");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
                return false;
                
            request = new HttpRequestMessage(HttpMethod.Get, ingestblocklisturl);
            response = client.SendAsync(request);
            response.Wait();
            if (response.Result.StatusCode != HttpStatusCode.NotFound)
                return false;

            string ingestblockurl = $"{screenshotresponse.UploadSasUrl}&comp=block&blockid=MDAwMDA=";
            request = new HttpRequestMessage(HttpMethod.Options, ingestblockurl);
            request.Headers.Add("Access-Control-Request-Method", "PUT");
            request.Headers.Add("Access-Control-Request-Headers", "x-ms-date,x-version");
            request.Headers.Add("Origin", "https://partner.microsoft.com");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
            {
                return false;
            }

            request = new HttpRequestMessage(HttpMethod.Put, ingestblockurl);
            byte[] screenshotdata = new byte[screenshot.Length];
            screenshot.Read(screenshotdata, 0, (int)(screenshot.Length));
            request.Content = new ByteArrayContent(screenshotdata);
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
            {
                return false;
            }

            //do some more block stuff, idk what the f*** this does or why it needs to be done
            request = new HttpRequestMessage(HttpMethod.Options, ingestblocklisturl);
            request.Headers.Add("Access-Control-Request-Method", "PUT");
            request.Headers.Add("Access-Control-Request-Headers", "x-ms-date,x-version");
            request.Headers.Add("Origin", "https://partner.microsoft.com");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
            {
                return false;
            }

            request = new HttpRequestMessage(HttpMethod.Put, ingestblocklisturl);
            request.Content = new StringContent("<?xml version=\"1.0\" encoding=\"utf-8\"?><BlockList><Latest>MDAwMDA=</Latest></BlockList>", System.Text.Encoding.UTF8, "application/xml");
            response = client.SendAsync(request);
            response.Wait();
            if (!response.Result.IsSuccessStatusCode)
            {
                return false;
            }

            return true;
        }

        static bool UploadFinished(string token, NeededUploadInfo neededuploadinfo, HttpMethod httpmethod)
        {
            var uriBuilder = new UriBuilder((xboxuri + $"/upload/finished/{neededuploadinfo.UploadInfo.XfusId}"));
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["callback"] = null;
            query["token"] = token;
            uriBuilder.Query = query.ToString();
            string uristring = uriBuilder.ToString();
            var request = new HttpRequestMessage(httpmethod, uristring);
            var response = client.SendAsync(request);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool CommitUpload(CommitalInfo commitalinfo)
        {
            var stringPayload = JsonConvert.SerializeObject(commitalinfo);
            var content = new StringContent(stringPayload, System.Text.Encoding.UTF8, "application/json");
            var response = client.PostAsync((partneruri.ToString() + $"/dashboard/packages/api/pkg/v2.0/packages/{commitalinfo.Id}/commit"), content);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static Identity GetIdentityInfo(NeededAppInfo neededappinfo)
        {
            var response = client.GetAsync((partneruri.ToString() + $"/dashboard/packages/api/pkg/v2.0/packageidentities?productbigid={neededappinfo.bigId}"));
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
            {
                string responseresult = response.Result.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<Identity>(responseresult);
            }
            else
            {
                return null;
            }
        }

        static string RunProcess(string fileName, string args)
        {
            string text = "";
            string result = "";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            System.Diagnostics.Process process2 = process;
            process2.Start();
            while (!process2.StandardOutput.EndOfStream)
            {
                text = process2.StandardOutput.ReadLine();
                if (text.Length > 0)
                {
                    result += text;
                }
            }
            return result;
        }
        //reused code from appx packer, its s*** I know but I don't have the energy to write soemthing better
        static bool MakeMsix(string inputfolder, string outputpath)
        {
            string makemsixpath = Path.Join(Directory.GetCurrentDirectory(), "makemsix", "makemsix");
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                makemsixpath = Path.Join(Directory.GetCurrentDirectory(), "MakeMsix", "MakeMsix.exe");

            string args = $"pack -d \"{inputfolder}\" -p \"{outputpath}\"";

            if (File.Exists(outputpath))
            {
                File.Delete(outputpath);
            }
            if (Directory.Exists(outputpath))
            {
                Directory.Delete(outputpath, true);
            }
            string output = RunProcess(makemsixpath, args).ToLower();
            if (!output.Contains("error:"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static Stream GenerateStreamFromString(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void appfile_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            if (StorePage != null)
                Process.Start(new ProcessStartInfo(StorePage) { UseShellExecute = true });
        }

        private void game_CheckedChanged(object sender, EventArgs e)
        {
            app.Checked = !game.Checked;
        }

        private void app_CheckedChanged(object sender, EventArgs e)
        {
            game.Checked = !app.Checked;
        }

        private void appPrivate_CheckedChanged(object sender, EventArgs e)
        {
            appPublic.Checked = !appPrivate.Checked;
            GroupBoxes.Enabled = appPrivate.Checked;
        }

        private void appPublic_CheckedChanged(object sender, EventArgs e)
        {
            appPrivate.Checked = !appPublic.Checked;
            GroupBoxes.Enabled = appPrivate.Checked;
        }

        private void namerandomised_CheckedChanged(object sender, EventArgs e)
        {
            namecustom.Checked = !namerandomised.Checked;
        }

        private void namecustom_CheckedChanged(object sender, EventArgs e)
        {
            namerandomised.Checked = !namecustom.Checked;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }
    }
}