// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class.

using System;
using System.Collections.Generic;
using Kontent.Ai.Delivery.Abstractions;

namespace Kontent.Statiq.Tests.Models
{
    public partial class HostedVideo
    {
        public const string Codename = "hosted_video";
        public const string VideoHostCodename = "video_host";
        public const string VideoIdCodename = "video_id";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IContentItemSystemAttributes System { get; set; }
        public IEnumerable<IMultipleChoiceOption> VideoHost { get; set; }
        public string VideoId { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}