using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Ontheheap
{
    public class PageS3Saver : IPageSaver
    {

        public string akid; 
        public string secret;
        public string bucketName;
        
        public void SaveStream(Stream data, string name, string contentType, string identifier)
        {
            var s3 = Amazon.AWSClientFactory.CreateAmazonS3Client(akid, secret);

            Amazon.S3.Model.PutObjectRequest request = new Amazon.S3.Model.PutObjectRequest();
            request.WithBucketName(bucketName)
                .WithContentType(contentType)
                .WithKey(identifier + "/" + name)
                .WithInputStream(data);


            Amazon.S3.Model.S3Response response = s3.PutObject(request);
            response.Dispose();
            s3.Dispose();
        }


    }
}
