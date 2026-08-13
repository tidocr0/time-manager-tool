import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5196/api'
});

export const getCategories = () => api.get('/categories');
export const getTasksByDate = (date) => api.get(`/tasks?date=${date}`);
export const createTask = (data) => api.post('/tasks', data);
export const updateTask = (id, data) => api.put(`/tasks/${id}`, data);
export const toggleTaskDone = (id) => api.patch(`/tasks/${id}/toggle-done`);
export const deleteTask = (id) => api.delete(`/tasks/${id}`);
export const getTaskAlerts = (date) => api.get(`/tasks/alerts?date=${date}`);
