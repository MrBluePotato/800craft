﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fCraft.ServerGUI {
    public partial class SkinViewer : Form {
        PlayerInfo player;
        Image webSkin;
        public SkinViewer ( fCraft.PlayerInfo player_ ) {
            InitializeComponent();
            player = player_;
            PlayerInfo info = player;
            GetSetSkin();
            SetPlayerInfoText(info);
        }

        void SetPlayerInfoText (PlayerInfo info) {
            textBox1.Text = "";
            PlayerLabel.Text = player.Name;
            if ( info.LastIP.Equals( System.Net.IPAddress.None ) ) {
                textBox1.Text += String.Format( "About {0}&S: Never seen before.\n", info.ClassyName );

            } else {
                if ( info != null ) {
                    TimeSpan idle = info.PlayerObject.IdleTime;
                    if ( info.IsHidden ) {
                        if ( idle.TotalMinutes > 2 ) {
                            if ( player.Can( Permission.ViewPlayerIPs ) ) {
                                textBox1.Text += String.Format( "About {0}&S: HIDDEN from {1} (idle {2})\n",
                                                info.ClassyName,
                                                info.LastIP,
                                                idle.ToMiniString() );
                            } else {
                                textBox1.Text += String.Format( "About {0}&S: HIDDEN (idle {1})\n",
                                                info.ClassyName,
                                                idle.ToMiniString() );
                            }
                        } else {
                            if ( player.Can( Permission.ViewPlayerIPs ) ) {
                                textBox1.Text += String.Format( "About {0}&S: HIDDEN. Online from {1}\n",
                                                info.ClassyName,
                                                info.LastIP );
                            } else {
                                textBox1.Text += String.Format( "About {0}&S: HIDDEN.\n",
                                                info.ClassyName );
                            }
                        }
                    } else {
                        if ( idle.TotalMinutes > 1 ) {
                            if ( player.Can( Permission.ViewPlayerIPs ) ) {
                                textBox1.Text += String.Format( "About {0}&S: Online now from {1} (idle {2})\n",
                                                info.ClassyName,
                                                info.LastIP,
                                                idle.ToMiniString() );
                            } else {
                                textBox1.Text += String.Format( "About {0}&S: Online now (idle {1})\n",
                                                info.ClassyName,
                                                idle.ToMiniString() );
                            }
                        } else {
                            if ( player.Can( Permission.ViewPlayerIPs ) ) {
                                textBox1.Text += String.Format( "About {0}&S: Online now from {1}\n",
                                                info.ClassyName,
                                                info.LastIP );
                            } else {
                                textBox1.Text += String.Format( "About {0}&S: Online now.\n",
                                                info.ClassyName );
                            }
                        }
                    }
                } else {
                    if ( player.Can( Permission.ViewPlayerIPs ) ) {
                        if ( info.LeaveReason != LeaveReason.Unknown ) {
                            textBox1.Text += String.Format( "About {0}&S: Last seen {1} ago from {2} ({3}).\n",
                                            info.ClassyName,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LastIP,
                                            info.LeaveReason );
                        } else {
                            textBox1.Text += String.Format( "About {0}&S: Last seen {1} ago from {2}.\n",
                                            info.ClassyName,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LastIP );
                        }
                    } else {
                        if ( info.LeaveReason != LeaveReason.Unknown ) {
                            textBox1.Text += String.Format( "About {0}&S: Last seen {1} ago ({2}).\n",
                                            info.ClassyName,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LeaveReason );
                        } else {
                            textBox1.Text += String.Format( "About {0}&S: Last seen {1} ago.\n",
                                            info.ClassyName,
                                            info.TimeSinceLastSeen.ToMiniString() );
                        }
                    }
                }
                // Show login information
                textBox1.Text += String.Format( "  Logged in {0} time(s) since {1:d MMM yyyy}.\n",
                                info.TimesVisited,
                                info.FirstLoginDate );
            }

            if ( info.IsFrozen ) {
                textBox1.Text += String.Format( "  Frozen {0} ago by {1}\n",
                                info.TimeSinceFrozen.ToMiniString(),
                                info.FrozenByClassy );
            }

            if ( info.IsMuted ) {
                textBox1.Text += String.Format( "  Muted for {0} by {1}\n",
                                info.TimeMutedLeft.ToMiniString(),
                                info.MutedByClassy );
                float blocks = ( ( info.BlocksBuilt + info.BlocksDrawn ) - info.BlocksDeleted );
                if ( blocks < 0 )
                    textBox1.Text += String.Format( "  &CWARNING! {0}&S has deleted more than built!\n", info.ClassyName );//<---- GlennMR on Au70 Galaxy
            }

            // Show ban information
            IPBanInfo ipBan = IPBanList.Get( info.LastIP );
            switch ( info.BanStatus ) {
                case BanStatus.Banned:
                    if ( ipBan != null ) {
                        textBox1.Text += String.Format( "  Account and IP are &CBANNED&S. See &H/BanInfo\n" );
                    } else {
                        textBox1.Text += String.Format( "  Account is &CBANNED&S. See &H/BanInfo\n" );
                    }
                    break;
                case BanStatus.IPBanExempt:
                    if ( ipBan != null ) {
                        textBox1.Text += String.Format( "  IP is &CBANNED&S, but account is exempt. See &H/BanInfo\n" );
                    } else {
                        textBox1.Text += String.Format( "  IP is not banned, and account is exempt. See &H/BanInfo\n" );
                    }
                    break;
                case BanStatus.NotBanned:
                    if ( ipBan != null ) {
                        textBox1.Text += String.Format( "  IP is &CBANNED&S. See &H/BanInfo\n" );

                    }
                    break;
            }


            if ( !info.LastIP.Equals( System.Net.IPAddress.None ) ) {
                // Show alts
                List<PlayerInfo> altNames = new List<PlayerInfo>();
                int bannedAltCount = 0;
                foreach ( PlayerInfo playerFromSameIP in PlayerDB.FindPlayers( info.LastIP ) ) {
                    if ( playerFromSameIP == info ) continue;
                    altNames.Add( playerFromSameIP );
                    if ( playerFromSameIP.IsBanned ) {
                        bannedAltCount++;
                    }
                }


                // Stats
                if ( info.BlocksDrawn > 500000000 ) {
                    textBox1.Text += String.Format( "  Built {0} and deleted {1} blocks, drew {2}M blocks, wrote {3} messages.\n",
                                    info.BlocksBuilt,
                                    info.BlocksDeleted,
                                    info.BlocksDrawn / 1000000,
                                    info.MessagesWritten );
                } else if ( info.BlocksDrawn > 500000 ) {
                    textBox1.Text += String.Format( "  Built {0} and deleted {1} blocks, drew {2}K blocks, wrote {3} messages.\n",
                                    info.BlocksBuilt,
                                    info.BlocksDeleted,
                                    info.BlocksDrawn / 1000,
                                    info.MessagesWritten );
                } else if ( info.BlocksDrawn > 0 ) {
                    textBox1.Text += String.Format( "  Built {0} and deleted {1} blocks, drew {2} blocks, wrote {3} messages.\n",
                                    info.BlocksBuilt,
                                    info.BlocksDeleted,
                                    info.BlocksDrawn,
                                    info.MessagesWritten );
                } else {
                    textBox1.Text += String.Format( "  Built {0} and deleted {1} blocks, wrote {2} messages.\n",
                                    info.BlocksBuilt,
                                    info.BlocksDeleted,
                                    info.MessagesWritten );
                }


                // More stats
                if ( info.TimesBannedOthers > 0 || info.TimesKickedOthers > 0 || info.PromoCount > 0 ) {
                    textBox1.Text += String.Format( "  Kicked {0}, Promoted {1} and banned {2} players.\n", info.TimesKickedOthers, info.PromoCount, info.TimesBannedOthers );
                }

                if ( info.TimesKicked > 0 ) {
                    if ( info.LastKickDate != DateTime.MinValue ) {
                        textBox1.Text += String.Format( "  Got kicked {0} times. Last kick {1} ago by {2}\n",
                                        info.TimesKicked,
                                        info.TimeSinceLastKick.ToMiniString(),
                                        info.LastKickByClassy );
                    } else {
                        textBox1.Text += String.Format( "  Got kicked {0} times.\n", info.TimesKicked );
                    }
                    if ( info.LastKickReason != null ) {
                        textBox1.Text += String.Format( "  Kick reason: {0}\n", info.LastKickReason );
                    }
                }


                // Promotion/demotion
                if ( info.PreviousRank == null ) {
                    if ( info.RankChangedBy == null ) {
                        textBox1.Text += String.Format( "  Rank is {0}&S (default).\n",
                                        info.Rank.ClassyName );
                    } else {
                        textBox1.Text += String.Format( "  Promoted to {0}&S by {1}&S {2} ago.\n",
                                        info.Rank.ClassyName,
                                        info.RankChangedByClassy,
                                        info.TimeSinceRankChange.ToMiniString() );
                        if ( info.RankChangeReason != null ) {
                            textBox1.Text += String.Format( "  Promotion reason: {0}\n", info.RankChangeReason );
                        }
                    }
                } else if ( info.PreviousRank <= info.Rank ) {
                    textBox1.Text += String.Format( "  Promoted from {0}&S to {1}&S by {2}&S {3} ago.\n",
                                    info.PreviousRank.ClassyName,
                                    info.Rank.ClassyName,
                                    info.RankChangedByClassy,
                                    info.TimeSinceRankChange.ToMiniString() );
                    if ( info.RankChangeReason != null ) {
                        textBox1.Text += String.Format( "  Promotion reason: {0}\n", info.RankChangeReason );
                    }
                } else {
                    textBox1.Text += String.Format( "  Demoted from {0}&S to {1}&S by {2}&S {3} ago.\n",
                                    info.PreviousRank.ClassyName,
                                    info.Rank.ClassyName,
                                    info.RankChangedByClassy,
                                    info.TimeSinceRankChange.ToMiniString() );
                    if ( info.RankChangeReason != null ) {
                        textBox1.Text += String.Format( "  Demotion reason: {0}\n", info.RankChangeReason );
                    }
                }

                if ( !info.LastIP.Equals( System.Net.IPAddress.None ) ) {
                    // Time on the server
                    TimeSpan totalTime = info.TotalTime;
                    if ( info != null ) {
                        totalTime = totalTime.Add( info.TimeSinceLastLogin );
                    }
                    textBox1.Text += String.Format( "  Spent a total of {0:F1} hours ({1:F1} minutes) here.\n",
                                    totalTime.TotalHours,
                                    totalTime.TotalMinutes );
                }
                textBox1.Text = Color.StripColors( textBox1.Text );
            }
        }
        void GetSetSkin() {
            GetSkin();
            pictureBox1.Image = CorrectSkin( webSkin );
            Rectangle rect = new Rectangle( 8, 8, 8, 8 );
            Rectangle rect2 = new Rectangle( 40, 8, 8, 8 );
            Image bitmap1 = cropImage( pictureBox1.Image, rect );
            Image bitmap2 = cropImage( pictureBox1.Image, rect2 );
            pictureBox1.Image = bitmap1;
            using ( Graphics g = Graphics.FromImage( pictureBox1.Image ) ) {
                g.DrawImage( bitmap2, new Point(0,0) );
            }
            pictureBox1.Image = ResizeBitmap( pictureBox1.Image as Bitmap, 128, 128 );
        }

        private static Bitmap ResizeBitmap ( Bitmap sourceBMP, int width, int height ) {
            Bitmap result = new Bitmap( width, height );
            using ( Graphics g = Graphics.FromImage( result ) ) {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage( sourceBMP, 0, 0, width, height );
            }
          
            return result;
        }

        private static Image cropImage ( Image img, Rectangle cropArea ) {
            Bitmap bmpImage = new Bitmap( img );
            Bitmap bmpCrop = bmpImage.Clone( cropArea,
            bmpImage.PixelFormat );
            return ( Image )( bmpCrop );
        }
        private static Image EmptySkin {
            get {
                Image image = ( Image )new Bitmap( 64, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
                using ( Graphics graphics = Graphics.FromImage( image ) ) {
                    graphics.Clear( System.Drawing.Color.Transparent );
                    using ( Brush brush1 = ( Brush )new SolidBrush( System.Drawing.Color.DimGray ) ) {
                        using ( Brush brush2 = ( Brush )new SolidBrush( System.Drawing.Color.DarkGray ) ) {
                            using ( Brush brush3 = ( Brush )new SolidBrush( System.Drawing.Color.LightGray ) ) {
                                graphics.FillRectangle( brush1, new Rectangle( 8, 0, 8, 8 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 4, 16, 4, 4 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 20, 16, 8, 4 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 44, 16, 4, 4 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 0, 8, 8, 8 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 16, 8, 8, 8 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 0, 20, 4, 12 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 8, 20, 4, 12 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 16, 20, 4, 12 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 28, 20, 4, 12 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 40, 20, 4, 12 ) );
                                graphics.FillRectangle( brush2, new Rectangle( 48, 20, 4, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 8, 8, 8, 8 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 24, 8, 8, 8 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 4, 20, 4, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 12, 20, 4, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 20, 20, 8, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 32, 20, 8, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 44, 20, 4, 12 ) );
                                graphics.FillRectangle( brush3, new Rectangle( 52, 20, 4, 12 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 16, 0, 8, 8 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 8, 16, 4, 4 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 28, 16, 8, 4 ) );
                                graphics.FillRectangle( brush1, new Rectangle( 48, 16, 4, 4 ) );
                            }
                        }
                    }
                }
                return image;
            }
        }

        private static void CopyRegionIntoImage ( Bitmap srcBitmap, Rectangle srcRegion, Bitmap destBitmap, Rectangle destRegion ) {
            using ( Graphics grD = Graphics.FromImage( destBitmap ) ) {
                grD.DrawImage( srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel );
            }
        }

        private Rectangle ConvertRectangleToImage ( Rectangle clicked, Image image ) {
            //get size of original image
            Size size = image.Size;
            //get value of scale
            float num = Math.Min( ( float )( ( ( float )ClientRectangle.Width ) / ( ( float )size.Width ) ), ( float )( ( ( float )ClientRectangle.Height ) / ( ( float )size.Height ) ) );
            //scale size to calculate translation
            size.Width = ( int )( size.Width * num );
            size.Height = ( int )( size.Height * num );
            //reverse translation
            clicked.X -= ( ClientRectangle.Width - size.Width ) / 2;
            clicked.Y -= ( ClientRectangle.Height - size.Height ) / 2;
            //reverse scale
            clicked.X = ( int )( clicked.X / num );
            clicked.Y = ( int )( clicked.Y / num );
            clicked.Width = ( int )( clicked.Width / num );
            clicked.Height = ( int )( clicked.Height / num );
            //return image coordinates
            return clicked;
        }
        private unsafe Image CorrectSkin ( Image skin ) {
            if ( skin == null )
                return EmptySkin;
            Bitmap bitmap = new Bitmap( skin );
            Rectangle rect = new Rectangle( 0, 0, bitmap.Width, bitmap.Height );
            System.Drawing.Imaging.BitmapData bitmapdata = bitmap.LockBits( rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
            for ( int index1 = 0; index1 < 16; ++index1 ) {
                byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                int index2 = 3;
                while ( index2 < 128 ) {
                    numPtr[index2] = byte.MaxValue;
                    index2 += 4;
                }
            }
            for ( int index1 = 16; index1 < 32; ++index1 ) {
                byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                int index2 = 3;
                while ( index2 < 256 ) {
                    numPtr[index2] = byte.MaxValue;
                    index2 += 4;
                }
            }
            bool flag = false;
            for ( int index1 = 0; index1 < 8; ++index1 ) {
                byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                int index2 = 163;
                while ( index2 < 224 ) {
                    if ( ( int )numPtr[index2] != ( int )byte.MaxValue )
                        flag = true;
                    index2 += 4;
                }
            }
            for ( int index1 = 8; index1 < 16; ++index1 ) {
                byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                int index2 = 131;
                while ( index2 < 256 ) {
                    if ( ( int )numPtr[index2] != ( int )byte.MaxValue )
                        flag = true;
                    index2 += 4;
                }
            }
            if ( !flag ) {
                for ( int index1 = 0; index1 < 8; ++index1 ) {
                    byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                    for ( int index2 = 163; index2 < 224; ++index2 )
                        numPtr[index2] = ( byte )0;
                }
                for ( int index1 = 8; index1 < 16; ++index1 ) {
                    byte* numPtr = ( byte* )( ( IntPtr )( void* )bitmapdata.Scan0 + ( index1 * bitmapdata.Stride ) );
                    for ( int index2 = 131; index2 < 256; ++index2 )
                        numPtr[index2] = ( byte )0;
                }
            }
            bitmap.UnlockBits( bitmapdata );
            return ( Image )bitmap;
        }
        private void GetSkin () {
            System.Net.WebClient webClient = new System.Net.WebClient();
            try {
                byte[] buffer;
                    buffer = webClient.DownloadData( "http://s3.amazonaws.com/MinecraftSkins/" + this.player.Name + ".png" );
                if ( this.webSkin != null )
                    this.webSkin.Dispose();
                this.webSkin = Image.FromStream( ( System.IO.Stream )new System.IO.MemoryStream( buffer ) );
            } catch {
                this.webSkin = ( Image )null;
            }
        }
    }
}