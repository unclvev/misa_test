import axios from 'axios';
import { BASE_API_LOCAL } from './urlbase';

// Axios instance tối giản (không xử lý token)
const Http = axios.create({
  baseURL: BASE_API_LOCAL,
  timeout: 100000,
  withCredentials: false,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Interceptor request (pass-through)
Http.interceptors.request.use(
  (config) => {
    // Đảm bảo Content-Type là application/json cho POST/PUT
    if (config.method === 'post' || config.method === 'put' || config.method === 'patch') {
      config.headers['Content-Type'] = 'application/json';
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor response (giữ nguyên response, để caller .data nếu muốn)
Http.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(error)
);

export default Http;


