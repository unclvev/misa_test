using AssetManagement.Application.Dtos;
using AssetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AssetListDto>>> GetAllAssets(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _assetService.GetAllAssetsPaged(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách tài sản", error = ex.Message });
        }
    }

    [HttpGet("filter")]
    public async Task<ActionResult<PagedResultDto<AssetListDto>>> FilterAssets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? departmentCode = null,
        [FromQuery] byte? typeCode = null)
    {
        try
        {
            var result = await _assetService.FilterAssets(pageNumber, pageSize, searchText, departmentCode, typeCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lọc tài sản", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AssetDto>> GetAssetById(ulong id)
    {
        try
        {
            var asset = await _assetService.GetAssetById(id);
            if (asset == null)
            {
                return NotFound(new { message = $"Không tìm thấy tài sản với ID {id}" });
            }
            return Ok(asset);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin tài sản", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<AssetDto>> CreateAsset([FromBody] RequestAssetDto createAssetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdAsset = await _assetService.CreateAsset(createAssetDto);
            return Ok(createdAsset);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo tài sản", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AssetDto>> UpdateAsset(ulong id, [FromBody] RequestAssetDto updateAssetDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedAsset = await _assetService.UpdateAsset(id, updateAssetDto);
            return Ok(updatedAsset);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật tài sản", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(ulong id)
    {
        try
        {
            var result = await _assetService.DeleteAsset(id);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy tài sản với ID {id}" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa tài sản", error = ex.Message });
        }
    }

    [HttpDelete("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] List<ulong> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest(new { message = "Danh sách ids rỗng." });

        try
        {
            var deletedCount = await _assetService.DeleteManyAsync(ids);
            var totalCount = ids.Count;
            var cannotDeleteCount = totalCount - deletedCount;
            
            return Ok(new 
            { 
                deletedCount = deletedCount,
                totalCount = totalCount,
                cannotDeleteCount = cannotDeleteCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa nhiều tài sản", error = ex.Message });
        }
    }


    [HttpGet("check-code/{assetCode}")]
    public async Task<ActionResult<bool>> CheckAssetCode(string assetCode)
    {
        var exists = await _assetService.IsAssetCodeExists(assetCode);
        return Ok(new { exists });
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        try
        {
            var departments = await _assetService.GetAllDepartments();
            return Ok(departments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách bộ phận", error = ex.Message });
        }
    }

    [HttpGet("asset-types")]
    public async Task<ActionResult<IEnumerable<AssetTypeDto>>> GetAssetTypes()
    {
        try
        {
            var assetTypes = await _assetService.GetAllAssetTypes();
            return Ok(assetTypes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách loại tài sản", error = ex.Message });
        }
    }

    [HttpGet("next-asset-code")]
    public async Task<ActionResult<string>> GetNextAssetCode()
    {
        try
        {
            var nextCode = await _assetService.GetNextAssetCode();
            return Ok(new { assetCode = nextCode });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy mã tài sản tiếp theo", error = ex.Message });
        }
    }

    [HttpGet("available-for-voucher")]
    public async Task<ActionResult<PagedResultDto<AssetListDto>>> GetAvailableAssetsForVoucher(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] string? departmentCode = null,
        [FromQuery] byte? typeCode = null)
    {
        try
        {
            var result = await _assetService.FilterAvailableForVoucher(pageNumber, pageSize, searchText, departmentCode, typeCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách tài sản khả dụng", error = ex.Message });
        }
    }
}

