// Các Model cũ của bạn giữ nguyên
export interface BanAnDto {
  maBan: number;
  soBan: string;
  trangThaiBan: string;
}

export interface DonHangNhanVienDto {
  maDonHang: number;
  maBan?: number;
  tenBan: string;
  loaiDonHang: string;
  ngayTao: string; 
  tongTien: number;
  trangThaiDon: string;
}

export interface NhanVienApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}

// ==========================================
// THÊM CÁC MODEL MỚI CHO GIAO DIỆN POS
// ==========================================

export interface ChiTietDonHangDto {
  maChiTiet: number;
  maMonAn: number;
  tenMonAn: string;
  soLuong: number;
  giaLucDat: number;
  thanhTien: number;
  ghiChu?: string;
  trangThaiBep?: string;
}

export interface DonHangHienTaiDto {
  maDonHang: number;
  maBan?: number;
  soBan?: string;
  tongTien: number;
  trangThaiDon?: string;
  trangThaiThanhToan?: string;
  chiTietDonHangs: ChiTietDonHangDto[];
}

// Tạm thời mô phỏng Model Món ăn (Nếu bạn đã có ở menu.model.ts thì có thể import vào)
export interface MonAn {
  maMonAn: number;
  tenMonAn: string;
  gia: number;
}