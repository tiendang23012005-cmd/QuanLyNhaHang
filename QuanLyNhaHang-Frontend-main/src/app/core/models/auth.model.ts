// Mô tả cấu trúc dữ liệu trả về từ API /api/auth/login hoặc /register
export interface AuthResponse {
  isSuccess: boolean;
  message: string;
  token?: string;
  fullName?: string;
  role?: string;
}

// Mô tả cấu trúc phiên làm việc của người dùng được lưu trong LocalStorage
export interface UserSession {
  fullName: string;
  role: string;
  token: string;
}