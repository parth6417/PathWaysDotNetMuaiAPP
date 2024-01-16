using Newtonsoft.Json;
using PathWays.Model;
using PathWays.Model.APIRespone;
using PathWaysVMS.API;
using Plugin.Maui.Audio;

namespace PathWays;

public partial class LogInUserDetails : ContentPage
{
    string id = string.Empty;
    private readonly IAudioManager audioManager;

    public LogInUserDetails(Person objperson, IAudioManager audioManager)
    {
        this.id = objperson.Person_Id;
        this.audioManager = audioManager;
        InitializeComponent();
        FullName.Text = objperson.First_Name + " " + objperson.Middle_Name + " " + objperson.Last_Name;
        txtEmail.Text = objperson.Email_1;
        Gender.Text = objperson.Gender;
        Phone.Text = objperson.Mobile_Phone;
        btnSaveAndNext.Text = "Save & Next";
    }

    private async void Close_Clicked(object sender, EventArgs e)
    {
        btnSaveAndNext.IsEnabled = false;
        if (txtWTM.Text == null || txtWTM.Text == "")
        {
            validWTM.IsVisible = true;
            btnSaveAndNext.IsEnabled = true;
            return;
        }
        else
        {
            validWTM.IsVisible = false;
        }

        var getResponse = await ApiAsync.CallApi("PersonLoginRecord/GetClockInPersonById/" + id, HttpMethod.Get);

        if (getResponse.Item2)
        {
            GetClockInPerson<PersonLoginRecord> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonLoginRecord>>(getResponse.Item1);
            await UpdateData(apiResponse.Data);
        }
        btnSaveAndNext.IsEnabled = true;

        App.Current.MainPage = new MainPage(this.audioManager);


        //this.Close(true);
    }


    public async Task<bool> UpdateData(PersonLoginRecord Data)
    {
        try
        {
            Data.VisitorPersonId = txtWTM.Text;
            string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            var postResponse = await ApiAsync.CallApi("PersonLoginRecord", HttpMethod.Put, jsonBody);
            if (postResponse.Item2)
                return true;
            else
                return false;
        }
        catch
        (Exception ex)
        {
            return false;
        }
    }
}