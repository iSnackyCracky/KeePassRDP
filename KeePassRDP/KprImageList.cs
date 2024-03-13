/*
 *  Copyright (C) 2018 - 2024 iSnackyCracky, NETertainer
 *
 *  This file is part of KeePassRDP.
 *
 *  KeePassRDP is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  KeePassRDP is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with KeePassRDP.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

using KeePass.UI;
using KeePassLib.Utility;
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;

namespace KeePassRDP
{
    [Designer(typeof(KprImageListDesigner))]
    [DesignerSerializer(typeof(KprImageListCodeDomSerializer), typeof(CodeDomSerializer))]
    [TypeConverter(typeof(KprImageListConverter))]
    [ToolboxItem(typeof(KprImageListToolboxItem))]
    [ToolboxBitmap(typeof(ImageList))]
    [ToolboxItemFilter("System.Windows.Forms")]
    [ImmutableObject(true)]
    public sealed class KprImageList : Component
    {
        private static readonly Lazy<ImageList> _singleInstance = new Lazy<ImageList>(() =>
        {
            var imageList1 = new ImageList
            {
                ImageStream = Resources.Resources.imageList1_ImageStream,
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(16, 16),
                TransparentColor = Color.Transparent
            };

            if (DpiUtil.ScalingRequired)
            {
                var size = new Size(DpiUtil.ScaleIntX(imageList1.ImageSize.Width), DpiUtil.ScaleIntY(imageList1.ImageSize.Height));
                var images = imageList1.Images.Cast<Image>().Select(image =>
                {
                    if (image.Height < size.Height)
                    {
                        using (image)
                            return GfxUtil.ScaleImage(image, size.Width, size.Height);
                    }
                    else
                        return image;
                }).ToArray();
                using (imageList1.ImageStream)
                    imageList1.ImageStream = null;
                imageList1.ImageSize = size;
                imageList1.Images.Clear();
                imageList1.Images.AddRange(images);
            }

            if (imageList1.Images.Count > 0)
            {
                try
                {
                    var names = Resources.Resources.imageList1_ImageKeys;
                    for (var i = 0; i < names.Length; i++)
                        imageList1.Images.SetKeyName(i, names[i]);
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            return imageList1;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly FieldInfo _imageInfoCollection = typeof(ImageList.ImageCollection).GetField("imageInfoCollection", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static ImageList Instance { get { return _singleInstance.Value; } }

        [ReadOnly(true)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        protected override bool CanRaiseEvents { get { return false; } }

        private readonly ImageList _imageList;

        public KprImageList(IContainer container) : this()
        {
            if (container != null)
            {
                try
                {
                    container.Add(_imageList, "imageList1");
                    container.Add(this, "kprImageList");
                }
                catch// (Exception e)
                {
                    //MessageBox.Show(e.Message);
                }
            }
        }

        public KprImageList()
        {
            var singleInstance = _singleInstance.Value;
            _imageList = new ImageList
            {
                ImageStream = singleInstance.ImageStream,
                ColorDepth = singleInstance.ColorDepth,
                ImageSize = singleInstance.ImageSize,
                TransparentColor = singleInstance.TransparentColor
            };
            if (_imageList.Images.Empty)
                _imageList.Images.AddRange(singleInstance.Images.Cast<Image>().ToArray());
            _imageInfoCollection.SetValue(_imageList.Images, _imageInfoCollection.GetValue(singleInstance.Images));
            //for (var i = 0; i < singleInstance.Images.Keys.Count; i++)
            //    _imageList.Images.SetKeyName(i, singleInstance.Images.Keys[i]);
        }

        public static implicit operator ImageList(KprImageList kprImageList)
        {
            return kprImageList._imageList;
        }
    }

    /// <summary>
    /// <see cref="ToolboxItem"/> for <see cref="KprImageList"/>.
    /// </summary>
    [Serializable]
    internal class KprImageListToolboxItem : ToolboxItem
    {
        public KprImageListToolboxItem(Type toolType) : base(toolType)
        {
        }

        KprImageListToolboxItem(SerializationInfo info, StreamingContext context)
        {
            Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost host)
        {
            return new IComponent[] { (KprImageList)host.CreateComponent(typeof(KprImageList)) };
        }
    }

    /// <summary>
    /// <see cref="ComponentConverter"/> for <see cref="KprImageList"/>.
    /// </summary>
    internal class KprImageListConverter : ComponentConverter
    {
        public KprImageListConverter() : base(typeof(KprImageList))
        {
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    /// <summary>
    /// <see cref="CodeDomSerializer"/> which skips serialization for the <see cref="ImageList"/> of <see cref="KprImageList"/>.
    /// </summary>
    internal class NoopCodeDomSerializer : CodeDomSerializer
    {
        public object Target { get; set; }
        public CodeDomSerializer OriginalSerializer { get; set; }

        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            if (codeObject is CodeStatementCollection &&
                ((CodeStatementCollection)codeObject).Count > 0 &&
                ((CodeStatementCollection)codeObject)[0] is CodeAssignStatement &&
                ((CodeAssignStatement)((CodeStatementCollection)codeObject)[0]).Left is CodeFieldReferenceExpression &&
                ((CodeFieldReferenceExpression)((CodeAssignStatement)((CodeStatementCollection)codeObject)[0]).Left).FieldName == "imageList1")
            {
                var container = manager.GetService(typeof(IContainer)) as IContainer;
                ImageList list = manager.GetInstance("kprImageList") as KprImageList ?? new KprImageList(container);

                return Target = list;
            }

            return OriginalSerializer.Deserialize(manager, codeObject);
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            if (value == Target || manager.GetName(value) == "imageList1")
            {
                Target = value;

                var rootCtx = manager.Context[typeof(RootContext)] as RootContext;

                if (GetExpression(manager, value) == null)
                    SetExpression(manager, value, new CodeFieldReferenceExpression(rootCtx.Expression, "imageList1"));

                return null;
            }

            return OriginalSerializer.Serialize(manager, value);
        }
    }

    /// <summary>
    /// <see cref="IDesignerSerializationProvider"/> for <see cref="KprImageList"/>.
    /// </summary>
    internal class KprImageListSerializationProvider : IDesignerSerializationProvider
    {
        public object Target { get; set; }
        public CodeDomSerializer OriginalSerializer { get; set; }

        public KprImageListSerializationProvider()
        {
            Target = null;
            OriginalSerializer = null;
        }

        public object GetSerializer(IDesignerSerializationManager manager, object currentSerializer, Type objectType, Type serializerType)
        {
            if (typeof(KprImageList).IsAssignableFrom(objectType))
                currentSerializer = currentSerializer is KprImageListCodeDomSerializer ? currentSerializer : new KprImageListCodeDomSerializer();

            if (typeof(ImageList).IsAssignableFrom(objectType))
            {
                currentSerializer = currentSerializer is NoopCodeDomSerializer ? currentSerializer : new NoopCodeDomSerializer();
                ((NoopCodeDomSerializer)currentSerializer).Target = Target;
                ((NoopCodeDomSerializer)currentSerializer).OriginalSerializer = OriginalSerializer;
            }

            return currentSerializer;
        }
    }

    /// <summary>
    /// <see cref="CodeDomSerializer"/> for <see cref="KprImageList"/>.
    /// </summary>
    [DefaultSerializationProvider(typeof(KprImageListSerializationProvider))]
    internal class KprImageListCodeDomSerializer : CodeDomSerializer
    {
        public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
        {
            var container = manager.GetService(typeof(IContainer)) as IContainer;
            ImageList list = manager.GetInstance("kprImageList") as KprImageList ?? new KprImageList(container);
            return list;

            /*var serializer = (CodeDomSerializer)manager.GetSerializer(typeof(KprImageList).BaseType, typeof(CodeDomSerializer));
            ImageList list = serializer.Deserialize(manager, codeObject) as KprImageList;
            return list;*/
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            var rootCtx = manager.Context[typeof(RootContext)] as RootContext;

            if (GetExpression(manager, manager.GetInstance("imageList1")) == null)
                SetExpression(manager, manager.GetInstance("imageList1"), new CodeFieldReferenceExpression(rootCtx.Expression, "imageList1"));

            var serializer = (CodeDomSerializer)manager.GetSerializer(typeof(KprImageList).BaseType, typeof(CodeDomSerializer));
            var codeObject = serializer.Serialize(manager, value);
            if (codeObject is CodeStatementCollection)
            {
                /*((CodeStatementCollection)codeObject).Add(new CodeVariableDeclarationStatement()
                {
                    Name = "imageList1",
                    Type = new CodeTypeReference(typeof(ImageList)),
                    InitExpression = new CodeCastExpression(typeof(ImageList), GetExpression(manager, value)),
                });*/
                ((CodeStatementCollection)codeObject).Add(new CodeAssignStatement(
                    new CodeFieldReferenceExpression(rootCtx.Expression, "imageList1"),
                    new CodeCastExpression(typeof(ImageList), GetExpression(manager, value))));
            }

            /*bool isComplete;
            codeObject = new CodeStatementCollection(new CodeStatement[]
            {
                new CodeAssignStatement(
                    new CodeVariableReferenceExpression("this.imageList1"),
                    new CodeCastExpression(typeof(ImageList), SerializeCreationExpression(manager, value, out isComplete)))
            });*/

            return codeObject;
        }
    }

    /// <summary>
    /// <see cref="ComponentDesigner"/> for <see cref="KprImageList"/>.
    /// </summary>
    internal class KprImageListDesigner : ComponentDesigner
    {
        private static readonly KprImageListSerializationProvider _provider = new KprImageListSerializationProvider();
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);

            //var ccs = (IComponentChangeService)GetService(typeof(IComponentChangeService));

            var manager = GetService(typeof(IDesignerSerializationManager)) as IDesignerSerializationManager;
            if (manager != null)
            {
                if (_provider.OriginalSerializer == null)
                    _provider.OriginalSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(ImageList), typeof(CodeDomSerializer));
                manager.AddSerializationProvider(_provider);
                try
                {
                    (manager as DesignerSerializationManager).SessionCreated -= FixLostReference;
                }
                catch
                {
                }
                (manager as DesignerSerializationManager).SessionCreated += FixLostReference;
                try
                {
                    manager.SerializationComplete -= FixLostReference;
                }
                catch
                {
                }
                manager.SerializationComplete += FixLostReference;
            }

            _provider.Target = (ImageList)(KprImageList)Component;

            var designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (designerHost == null)
                return;

            var innerListProperty = designerHost.Container.Components.GetType().GetProperty("InnerList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var innerList = innerListProperty.GetValue(designerHost.Container.Components, null) as ArrayList;

            if (innerList == null)
                return;

            var idx = innerList.IndexOf(component);
            var imageList = innerList[idx - 1];

            if (idx <= 0)
                return;

            // Make sure KprImageList is the very first component.
            innerList.Remove(imageList);
            innerList.Insert(0, imageList);

            innerList.Remove(component);
            innerList.Insert(0, component);
        }

        protected override void Dispose(bool disposing)
        {
            var manager = GetService(typeof(IDesignerSerializationManager)) as IDesignerSerializationManager;
            if (manager != null)
            {
                (manager as DesignerSerializationManager).SessionCreated -= FixLostReference;
                manager.SerializationComplete -= FixLostReference;
            }
            base.Dispose(disposing);
        }

        // Try to repair references to KprImageList in case they got lost somewhere.
        private void FixLostReference(object sender, EventArgs e)
        {
            _provider.Target = (ImageList)(KprImageList)Component;

            var designerHost = GetService(typeof(IDesignerHost)) as IDesignerHost;

            if (designerHost == null)
                return;

            var repaired = false;
            for (var i = 0; i < designerHost.Container.Components.Count; i++)
            {
                var otherComponent = designerHost.Container.Components[i];
                if (otherComponent == null || otherComponent == Component || otherComponent == (ImageList)(KprImageList)Component)
                    continue;
                var imageKeyProp = otherComponent
                    .GetType()
                    .GetProperty("ImageKey", BindingFlags.Instance | BindingFlags.Public);
                if (imageKeyProp == null)
                    continue;
                var imageKeyValue = imageKeyProp.GetValue(otherComponent, null) as string;
                if (string.IsNullOrEmpty(imageKeyValue))
                    continue;
                if (!((ImageList)(KprImageList)Component).Images.ContainsKey(imageKeyValue))
                    continue;
                var imageListProp = otherComponent
                    .GetType()
                    .GetProperty("ImageList", BindingFlags.Instance | BindingFlags.Public);
                if (imageListProp == null && otherComponent is Control && (otherComponent as Control).Parent != null)
                {
                    otherComponent = (otherComponent as Control).Parent;
                    imageListProp = otherComponent
                        .GetType()
                        .GetProperty("ImageList", BindingFlags.Instance | BindingFlags.Public);
                }
                if (imageListProp == null)
                    continue;
                var imageListValue = imageListProp.GetValue(otherComponent, null) as ImageList;
                if (imageListValue == null)
                {
                    repaired = true;
                    imageListProp.SetValue(otherComponent, (ImageList)(KprImageList)Component, null);
                }
            }

            if (repaired)
                RaiseComponentChanged(TypeDescriptor.GetProperties(Component)["DesignMode"], null, null);
        }
    }
}