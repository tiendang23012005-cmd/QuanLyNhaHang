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