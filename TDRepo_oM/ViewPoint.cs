﻿using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.External.TDRepo
{
    public class Viewpoint: IObject // See https://3drepo.github.io/3drepo.io/#api-Issues-newIssue
    {
        [Description("The right vector of the viewpoint indicating the direction of right in relative coordinates.")]
        public virtual double[] Right { get; set; }

        [Description("The up vector of the viewpoint indicating the direction of up in relative coordinates.")]
        public virtual double[] Up { get; set; }

        [Description("The position vector indicates where in the world the viewpoint is positioned.")]
        public virtual double[] Position { get; set; }

        [Description("The vector indicating where in the world the viewpoint is looking at.")]
        public virtual double[] LookAt { get; set; }

        [Description("The vector indicating where is the viewpoint is looking at in relative coordinates.")]
        public virtual double[] ViewDirection { get; set; }

        [Description("The vector indicating the near plane.")]
        public virtual double NearPlane { get; set; }

        [Description("The vector indicating the far plane.")]
        public virtual double FarPlane { get; set; }

        [Description("The angle of the field of view.")]
        public virtual double FOV { get; set; }

        [Description("The aspect ratio of the fustrum.")]
        public virtual double AspectRatio { get; set; }

        [Description("If the issue is associated with one or more objects from the model this field has the value of a group id generated to hold those objects")]
        public virtual string HighlightedGroupId { get; set; }

        [Description("The aspect ratio of the fustrum.")]
        public virtual bool HideIFC { get; set; }

        [Description("A string in base64 representing the screenshot associated with the issue.")]
        public virtual string Screenshot { get; set; }
    }
}
