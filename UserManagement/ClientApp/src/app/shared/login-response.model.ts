export class LoginResponse {
  success: boolean;
  token: string;
  expiresInMinutes: number;
  message: string;
  email: string;
  role: string;
}
