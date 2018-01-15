using GGCostWebAPI.Handlers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;

namespace GGCostWebAPI.Controllers
{
    [Route("api/googledrive")]
    public class GoogleDriveController : Controller
    {
        GoogleDrive mGoogleDrive = new GoogleDrive();

        // GET api/googledrive
        [HttpGet]
        public byte[] Get()
        {
            return mGoogleDrive.LoadFile();
        }

        [HttpPost]
        public IActionResult Post()
        {
            try
            {
                var form = Request.Form;
                if (form.Files.Count == 0)
                {
                    throw new FileNotFoundException("업로드할 파일을 찾지 못했습니다");
                }

                foreach (var formFile in form.Files)
                {
                    if (formFile.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            formFile.CopyTo(ms);
                            mGoogleDrive.SaveFile(ms.ToArray());
                        }
                    }
                }

                return StatusCode((int)HttpStatusCode.OK, "OK");
            }
            catch (FileNotFoundException ex)
            {
                return StatusCode((int)HttpStatusCode.NoContent, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
