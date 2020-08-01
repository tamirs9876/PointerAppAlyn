using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Alyn.Pointer.Common
{
    public static class Extensions
    {
        public static IEnumerable<Control> DescendentsFromPoint(this Control control, Point point)
        {
            var ctrl = control.GetChildAtPoint(point);
            if (ctrl != null)
            {
                yield return ctrl;

                var screenPt = control.PointToScreen(point);
                point = ctrl.PointToScreen(screenPt);

                foreach (var item in DescendentsFromPoint(ctrl, point))
                {
                    yield return item;
                }
            }
        }
    }
}
