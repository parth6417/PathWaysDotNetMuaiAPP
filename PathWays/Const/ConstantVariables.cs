using Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathWays.Const
{
    public static class ConstantVariables
    {
        public static RegionEndpoint region = RegionEndpoint.APSouth1;

        public static string CollectionId = "face_recognition_collection";

        public static string TableName = "face_recognition";

        public static string TableFieldName1 = "RekognitionId";

        public static string TableFieldName2 = "FullName";

        public static string BacketName = "person-name";

        public static string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "TakeAndChooseImages");

        public static string awsAccessKeyId = "AKIAUYDBMDWBUJNTWK7P123";

        public static string awsSecretAccessKey = "RB/ZMz5/lyP9lZZYgY+tW5pLFc3SN+T4FNeuDMkc123";

        public static string APIURL = "https://3bad-54-89-66-126.ngrok-free.app/api/";
    }
}
