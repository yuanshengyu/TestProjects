using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

//http://www.codeproject.com/Articles/26814/C-Scrollable-Picturebox-Custom-Control
namespace XPicbox
{

    /// <summary>
    /// Extend the panel control to provide a
    /// scrollable picturebox control
    /// </summary>
    public partial class XtendPicBox : Panel
    {

        #region Local Member Variables


        PictureBox innerPicture = new PictureBox();
        private bool mAutoScroll = true;

        #endregion


        #region Constructor


        /// <summary>
        /// Constructor - set up the inner
        /// picture box so that is plants itself
        /// in the top left corner of the panel 
        /// and so that its size mode is always
        /// set to normal.  At normal, the picture
        /// will appear at full size.
        /// </summary>
        public XtendPicBox()
        {
            InitializeComponent();

            // add the inner picture
            innerPicture.Top = 0;
            innerPicture.Left = 0;
            innerPicture.SizeMode = PictureBoxSizeMode.Normal;
            innerPicture.MouseClick += new MouseEventHandler(OnPictureMouseClick);
            innerPicture.MouseMove += new MouseEventHandler(OnPictureMouseMove);
            innerPicture.MouseDoubleClick += new MouseEventHandler(OnPictureMouseDoubleClick);

            Controls.Add(innerPicture);
        }

        private void OnPictureMouseDoubleClick(object sender, MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        private void OnPictureMouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        private void OnPictureMouseClick(object sender, MouseEventArgs e)
        {
            base.OnMouseClick(e);
        }

        #endregion


        #region Properties


        /// <summary>
        /// Allow control consumer to set the image
        /// used in the internal picture box through
        /// this public and browsable property -
        /// set the editor to the file name editor
        /// to make the control easier to use at
        /// design time (to provide an interface for
        /// browsing to and selecting the image file
        /// </summary>
        [Category("Image File")]
        [Browsable(true)]
        [Description("Set path to image file.")]
        public Image Image
        {
            get
            {
                return innerPicture.Image;
            }
            set
            {
                innerPicture.Image = value;

                // resize the image to match the image file
                if (innerPicture.Image != null)
                    innerPicture.Size = innerPicture.Image.Size;
            }
        }

        public PictureBox InnerPicture
        {
            get { return innerPicture; }
        }

        /// <summary>
        /// Override the autoscroll property
        /// and use the browsable attribute
        /// to hide it from control consumer -
        /// The property will always be set
        /// to true so that the picturebox will
        /// always scroll
        /// </summary>
        [Browsable(false)]
        public override bool AutoScroll
        {
            get
            {
                return mAutoScroll;
            }
            set
            {
                mAutoScroll = value;
            }
        }

        #endregion


    }
}
