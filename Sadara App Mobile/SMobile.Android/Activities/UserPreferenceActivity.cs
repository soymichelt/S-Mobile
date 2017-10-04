﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android;
using SMobile.Android.Configuration;
using Firebase;
using Android.Gms.Common;
using Android.Util;
using System.Threading.Tasks;
using Firebase.Iid;

namespace SMobile.Android.Activities
{
    
    [Activity(Label = "UserPreferenceActivity", MainLauncher = true, Icon = "@drawable/ic_isotipo_sadara")]
    public class UserPreferenceActivity : Activity
    {

        ProgressBar progressBar;

        Helpers.UserPreferenceRecyclerViewAdapter adapter;

        RecyclerView recyclerView;

        RecyclerView.LayoutManager layoutManger;

        Button btnPreferences;

        List<Models.Entities.PreferenceSelectedEntity> preferences =
            new List<Models.Entities.PreferenceSelectedEntity>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestPermissions(

                new string[] {

                    Manifest.Permission.GetAccounts,

                    Manifest.Permission.UseCredentials,

                    Manifest.Permission.Internet,

                    Manifest.Permission.ReadContacts,

                    Manifest.Permission.Camera

                },

                0

            );

            // Create your application here
            this.SetContentView(Resource.Layout.UserPreference);

            this.StartFirebase();

            this.progressBar = FindViewById<ProgressBar>(Resource.Id.userPreferenceProgressBar);

            this.recyclerView = FindViewById<RecyclerView>(Resource.Id.preferencesRecyclerView);

            this.LoadUsersPreferencesList();
            
            //this.ManageIntent();

            Task.Run(() => {

                //var instanceId = FirebaseInstanceId.Instance;

                //instanceId.DeleteInstanceId();

            });

            //Selección de Preferencias
            this.btnPreferences = FindViewById<Button>(Resource.Id.selectPreferencesButton);

            this.btnPreferences.Click += (e, handler) => {

                var selectList = this.preferences.Where(c => c.selected).ToList();

                selectList.ForEach((item) => {

                    Models.FirebaseModel.PreferenceWithUserModel preferenceWithUserModel = new Models.FirebaseModel.PreferenceWithUserModel();

                    preferenceWithUserModel.Add(new Models.Entities.PreferenceWithUserEntity() {

                        uid = "Ingresar Uid del Usuario",

                        uidPreference = item.uid,

                    });

                });

            };

        }

        public string GetTokenForApp()
        {

            var instanceId = Firebase.Iid.FirebaseInstanceId.GetInstance(Configuration.FirebaseConfig.app);

            return instanceId.Token;

        }

        public void SubscribeToTopic(string topic)
        {
            if (this.IsPlayServicesAvailable())
            {

                Firebase.Messaging.FirebaseMessaging.Instance.SubscribeToTopic(topic);

            }
            else
            {

                Toast.MakeText(this, this.msgText, ToastLength.Long).Show();

            }

        }

        string msgText;

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);            
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)) {
                    msgText = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                }
                else
                {
                    msgText = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                msgText = "Google Play Services is available.";
                return true;
            }
        }

        public void ManageIntent()
        {

            if (Intent.Extras != null)
            {

                foreach (var key in Intent.Extras.KeySet())
                {

                    var value = Intent.Extras.GetString(key);

                    Log.Debug("SadaraFirebaseIIDService", "Key: {0} Value: {1}", key, value);

                }

            }

        }

        private void StartFirebase()
        {

            if (FirebaseConfig.app == null)
                FirebaseConfig.app = FirebaseApp.InitializeApp(this, FirebaseConfig.firebaseOptions, "Sadara Mobile");

        }

        private async void LoadUsersPreferencesList()
        {
            var model = new Models.FirebaseModel.PreferenceModel();

            this.ShowProgressBar(true);

            var list = await model.List();
            
            this.preferences.Clear();

            foreach (var item in list)
            {
                this.preferences.Add(new Models.Entities.PreferenceSelectedEntity() {
                    uid = item.uid,
                    name = item.name
                });
            }

            this.adapter = new Helpers.UserPreferenceRecyclerViewAdapter(this.preferences);

            recyclerView.HasFixedSize = true;

            this.layoutManger = new LinearLayoutManager(this);

            recyclerView.SetLayoutManager(this.layoutManger);

            recyclerView.SetAdapter(this.adapter);

            this.ShowProgressBar(false);

        }

        private void ShowProgressBar(Boolean IsVisible)
        {

            progressBar.Visibility = IsVisible ? ViewStates.Visible : ViewStates.Invisible;

        }
    }
}