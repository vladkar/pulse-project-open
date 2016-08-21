using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.Converter.Attribute;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.Converter
{
    public class Generator
    {
        public static String getName(Type t)
        {
            ClassAttribute MyAttribute =
                (ClassAttribute) System.Attribute.GetCustomAttribute(t, typeof(ClassAttribute));
            if (MyAttribute != null)
            {
                return MyAttribute.name;
            }
            return null;
        }

        public static Frame getControlElement(object objectUI, FrameProcessor ui)
        {
            Type typeObject = objectUI.GetType();
            
            MemberInfo[] memberInfo = typeObject.GetMembers().Where(m => m.DeclaringType != typeof(object)).ToArray();
            var nameClient = getName(typeObject);
            TreeNode treeNode = FrameHelper.createTreeNode(ui, 0, 0,
                ConstantFrameUI.controlsFrameUIWidth, ConstantFrameUI.controlsTreeNodeHeight,
                nameClient ?? "",
                 ColorConstant.ScenarioBackColorButton);
            treeNode.TextOffsetX = ConstantFrameUI.controlsPaddingLeft;
            treeNode.OffsetChild = ConstantFrameUI.offsetXChild;
            treeNode.Font = ConstantFrameUI.segoeReg20;

            foreach (MemberInfo member in memberInfo)
            {
                var widthElement = ConstantFrameUI.controlsFrameUIWidth - treeNode.TextOffsetX - treeNode.OffsetChild-5;
                var heightElement = 40;
                
                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                    {
                        UIAttribute attr = getAttributes(member);
                        Frame frameElement = attr.CreateFrame(ui, member, objectUI, 0, 0, widthElement, heightElement);
                        treeNode.addNode(frameElement);
                        break;
                    }

                    case MemberTypes.Method:
                    {
                        UIAttribute attr = getAttributes(member);
                        if (attr == null)
                        {
                            break;
                        }
                        var button = attr.CreateFrame(ui, member, objectUI, 0, 0, widthElement, heightElement);
                        treeNode.addNode(button);
                        break;
                    }
                }
            }
            return treeNode;
        }



        static UIAttribute getAttributes(MemberInfo type)
        {
            var attributes = type.GetCustomAttributes(true);
            return attributes.Length != 0 ? attributes.Select(attribute => attribute as UIAttribute).FirstOrDefault() : null;
        }
    }
}
