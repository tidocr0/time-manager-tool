<template>
  <div class="day-view-container">
    <div class="header">
      <div class="date-controls">
        <button class="nav-btn" @click="prevDay">&lt;</button>
        <h1 class="date-title">{{ formattedDate }}</h1>
        <button class="nav-btn" @click="nextDay">&gt;</button>
        <button class="today-btn" v-if="currentDate !== defaultDate" @click="goToToday">Hôm nay</button>
      </div>
      <button class="add-btn" @click="openAddModal">+</button>
    </div>

    <div class="filters-container">
      <div class="filter-group">
        <label>Danh mục</label>
        <select v-model="filterCategory">
          <option value="all">Tất cả</option>
          <option v-for="cat in categories" :key="cat.categoryId" :value="cat.categoryId">
            {{ cat.name }}
          </option>
        </select>
      </div>
      
      <div class="filter-group">
        <label>Mức độ</label>
        <select v-model="filterPriority">
          <option value="all">Tất cả</option>
          <option :value="1">Khẩn</option>
          <option :value="0">Bình thường</option>
        </select>
      </div>
    </div>

    <div class="task-list">
      <div 
        v-for="task in filteredAndSortedTasks" 
        :key="task.taskId" 
        class="task-item"
        :class="{ 'is-done': task.isDone, 'clickable': true }"
        @click="openViewModal(task)"
      >
        <span class="category-badge" :style="{ color: task.category?.colorHex, backgroundColor: task.category?.colorHex + '20' }">{{ task.category?.name }}</span>
        
        <div class="task-content">
          <input 
            type="checkbox" 
            class="task-checkbox" 
            :checked="task.isDone" 
            @change="handleToggleDone(task)" 
            @click.stop
          />
          <div class="task-text">
            <span class="task-title">{{ task.title }}</span>
            <span v-if="task.note" class="task-note-preview">{{ task.note }}</span>
          </div>
        </div>

        <div class="task-actions">
          <div class="badge-group">
            <span class="deadline-label" :class="getDeadlineStatusInfo(task).class">
              {{ getDeadlineStatusInfo(task).text }}
            </span>
            <span class="priority-label" :class="{ 'urgent': task.priority === 1 }">
              {{ task.priority === 1 ? 'Khẩn' : 'Bình thường' }}
            </span>
          </div>
          <button class="delete-btn" @click.stop="handleDelete(task.taskId)" title="Xoá task">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
              <line x1="10" y1="11" x2="10" y2="17"></line>
              <line x1="14" y1="11" x2="14" y2="17"></line>
            </svg>
          </button>
        </div>
      </div>
      
      <div v-if="filteredAndSortedTasks.length === 0" class="empty-state">
        Không có công việc nào cho hôm nay!
      </div>
    </div>

    <!-- Modal Overlay -->
    <div v-if="showModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <h2 v-if="isReadOnly">Chi tiết công việc</h2>
        <h2 v-else>{{ isEditing ? 'Sửa công việc' : 'Thêm công việc mới' }}</h2>
        
        <div v-if="isReadOnly" class="read-only-details">
          <div class="detail-row">
            <span class="detail-label">Tiêu đề</span>
            <span class="detail-value">{{ newTask.title }}</span>
          </div>
          <div class="detail-row" v-if="newTask.note">
            <span class="detail-label">Việc cần làm</span>
            <span class="detail-value note-value">{{ newTask.note }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Danh mục</span>
            <span class="detail-value">
              {{ getCategoryName(newTask.categoryId) }}
            </span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Mức độ</span>
            <span class="detail-value" :class="{'urgent-text': newTask.priority === 1}">{{ newTask.priority === 1 ? 'Khẩn' : 'Bình thường' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Ngày hạn</span>
            <span class="detail-value">{{ formatDeadlineStr(newTask.deadlineDateTime?.split('T')[0], newTask.deadlineDateTime?.split('T')[1]) }}</span>
          </div>
          <div class="detail-row" v-if="newTask.startDate">
            <span class="detail-label">Ngày bắt đầu</span>
            <span class="detail-value">{{ formatDateStr(newTask.startDate) }}</span>
          </div>
          
          <div class="detail-row" v-if="newTask.isRecurring">
            <span class="detail-label">Lặp lại</span>
            <span class="detail-value">Hàng tuần</span>
          </div>
          
          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="closeModal">Đóng</button>
            <button type="button" class="btn-submit" @click="switchToEdit">Sửa công việc</button>
          </div>
        </div>

        <form v-else @submit.prevent="submitTask">
          <div class="form-group">
            <label>Tiêu đề <span class="required">*</span></label>
            <input type="text" v-model="newTask.title" required placeholder="Nhập tiêu đề công việc" />
          </div>
          
          <div class="form-group">
            <label>Việc cần làm</label>
            <textarea v-model="newTask.note" rows="3" placeholder="Chi tiết thêm..."></textarea>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Danh mục</label>
              <select v-model="newTask.categoryId">
                <option v-for="cat in categories" :key="cat.categoryId" :value="cat.categoryId">
                  {{ cat.name }}
                </option>
              </select>
            </div>
            
            <div class="form-group">
              <label>Mức độ</label>
              <select v-model="newTask.priority">
                <option :value="0">Bình thường</option>
                <option :value="1">Khẩn</option>
              </select>
            </div>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Ngày bắt đầu</label>
              <input type="date" v-model="newTask.startDate" />
            </div>
            
            <div class="form-group">
              <label>Ngày hạn</label>
              <input type="datetime-local" v-model="newTask.deadlineDateTime" required />
            </div>
          </div>

          <div class="form-group" v-if="!isEditing" style="margin-top: -4px;">
            <label style="display: flex; align-items: center; gap: 8px; font-weight: 500; cursor: pointer; flex-direction: row;">
              <input type="checkbox" v-model="newTask.isRecurring" style="width: 16px; height: 16px; margin: 0; cursor: pointer;" />
              Lặp lại hàng tuần
            </label>
            <p v-if="newTask.isRecurring" style="font-size: 12px; color: #64748b; margin: 6px 0 0 24px; font-weight: 400;">
              Sẽ tự tạo 8 tuần liên tiếp vào {{ recurringDayText }}, bắt đầu từ ngày đã chọn.
            </p>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-cancel" @click="closeModal">Huỷ</button>
            <button type="submit" class="btn-submit">Lưu công việc</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, onUnmounted, computed } from 'vue';
import { getCategories, getTasksByDate, createTask, updateTask, toggleTaskDone, deleteTask, getTaskAlerts } from '../services/api';

export default {
  name: 'DayView',
  setup() {
    const tasks = ref([]);
    const categories = ref([]);
    const showModal = ref(false);
    const isEditing = ref(false);
    const isReadOnly = ref(false);
    const editTaskId = ref(null);
    
    // Default form state
    const defaultDate = new Date().toISOString().split('T')[0];
    const currentDate = ref(defaultDate);
    let notificationInterval = null;
    let midnightTimeout = null;

    const filterCategory = ref('all');
    const filterPriority = ref('all');
    
    const initialForm = {
      title: '',
      note: '',
      categoryId: null,
      priority: 0,
      deadlineDateTime: '',
      startDate: null,
      isRecurring: false,
      recurrenceDayOfWeek: null
    };
    
    const newTask = ref({ ...initialForm });

    const formattedDate = computed(() => {
      const today = new Date();
      const todayStr = today.toISOString().split('T')[0];
      
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const tomorrowStr = tomorrow.toISOString().split('T')[0];
      
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      const yesterdayStr = yesterday.toISOString().split('T')[0];

      const [y, m, d] = currentDate.value.split('-');
      const targetDate = new Date(y, m - 1, d);
      const dayOfWeek = targetDate.getDay() === 0 ? 'Chủ nhật' : `Thứ ${targetDate.getDay() + 1}`;
      const dateString = targetDate.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });

      if (currentDate.value === todayStr) {
        return `Hôm nay - ${dayOfWeek}, ${dateString}`;
      } else if (currentDate.value === tomorrowStr) {
        return `Ngày mai - ${dayOfWeek}, ${dateString}`;
      } else if (currentDate.value === yesterdayStr) {
        return `Hôm qua - ${dayOfWeek}, ${dateString}`;
      }
      return `${dayOfWeek}, ${dateString}`;
    });

    const getCategoryName = (id) => {
      const cat = categories.value.find(c => c.categoryId === id);
      return cat ? cat.name : 'Không xác định';
    };

    const recurringDayText = computed(() => {
      const ddt = newTask.value.deadlineDateTime;
      let dDate = '';
      if (ddt) {
        if (ddt.includes('T')) {
          dDate = ddt.split('T')[0];
        } else {
          dDate = ddt;
        }
      }
      
      const targetDateStr = newTask.value.startDate || dDate;
      if (!targetDateStr) return 'ngày đã chọn';
      
      const [y, m, d] = targetDateStr.split('-');
      const targetDate = new Date(y, m - 1, d);
      if (isNaN(targetDate)) return 'ngày đã chọn';
      return targetDate.getDay() === 0 ? 'Chủ nhật' : `Thứ ${targetDate.getDay() + 1}`;
    });

    const getCategoryColor = (id) => {
      const cat = categories.value.find(c => c.categoryId === id);
      return cat ? cat.colorHex : '#ccc';
    };

    const formatDateStr = (dateStr) => {
      if (!dateStr) return '';
      const [y, m, d] = dateStr.split('T')[0].split('-');
      return `${d}/${m}/${y}`;
    };

    const formatDeadlineStr = (dateStr, timeStr) => {
      if (!dateStr) return '';
      const [y, m, d] = dateStr.split('T')[0].split('-');
      const targetDate = new Date(y, m - 1, d);
      const dayOfWeek = targetDate.getDay() === 0 ? 'Chủ nhật' : `Thứ ${targetDate.getDay() + 1}`;
      const dateString = `${d}/${m}/${y}`;
      
      if (timeStr) {
        const hm = timeStr.split(':').slice(0, 2).join(':');
        return `${dayOfWeek}, ${dateString} - ${hm}`;
      }
      return `${dayOfWeek}, ${dateString}`;
    };

    const getTaskDiffDays = (task) => {
      if (!task.deadlineDate) return 999999;
      
      const [cy, cm, cd] = currentDate.value.split('-');
      const current = new Date(cy, cm - 1, cd);
      
      const [dy, dm, dd] = task.deadlineDate.split('T')[0].split('-');
      const deadline = new Date(dy, dm - 1, dd);
      
      const diffTime = deadline - current;
      return Math.round(diffTime / (1000 * 60 * 60 * 24));
    };

    const getDeadlineStatusInfo = (task) => {
      if (!task.deadlineDate) return { text: '', class: '' };
      
      const diffDays = getTaskDiffDays(task);
      
      if (diffDays < 0) {
        return { text: `Quá hạn ${-diffDays} ngày`, class: 'overdue' };
      } else if (diffDays === 0) {
        return { text: 'Đến hạn hôm nay', class: 'today' };
      } else {
        if (diffDays > 14) {
          const weeks = Math.round(diffDays / 7);
          return { text: `Còn ${weeks} tuần`, class: 'normal' };
        }
        return { text: `Còn ${diffDays} ngày`, class: 'normal' };
      }
    };

    const filteredAndSortedTasks = computed(() => {
      let result = tasks.value;
      
      if (filterCategory.value !== 'all') {
        result = result.filter(t => t.categoryId === filterCategory.value);
      }
      
      if (filterPriority.value !== 'all') {
        result = result.filter(t => t.priority === filterPriority.value);
      }
      
      return result.slice().sort((a, b) => getTaskDiffDays(a) - getTaskDiffDays(b));
    });

    const loadData = async () => {
      try {
        const catRes = await getCategories();
        categories.value = catRes.data;
        if (categories.value.length > 0) {
          newTask.value.categoryId = categories.value[0].categoryId;
        }

        await loadTasks();
      } catch (error) {
        console.error('Error loading data:', error);
      }
    };

    const loadTasks = async () => {
      try {
        const res = await getTasksByDate(currentDate.value);
        tasks.value = res.data;
      } catch (error) {
        console.error('Error loading tasks:', error);
      }
    };

    const requestNotificationPermission = async () => {
      if (!('Notification' in window)) return;
      
      const permission = await Notification.requestPermission();
      if (permission === 'granted') {
        checkAlertsForNotification();
        notificationInterval = setInterval(() => {
          checkAlertsForNotification();
        }, 1800000);
      }
    };

    const checkAlertsForNotification = async () => {
      try {
        const todayStr = new Date().toISOString().split('T')[0];
        const res = await getTaskAlerts(todayStr);
        if (res.data && res.data.length > 0) {
          const storageKey = `notifiedTasks_${todayStr}`;
          const storedIds = JSON.parse(localStorage.getItem(storageKey) || '[]');
          
          const newTasks = res.data.filter(t => !storedIds.includes(t.taskId));
          
          if (newTasks.length > 0) {
            let bodyText = res.data.slice(0, 8).map(task => {
              const priorityText = task.priority === 1 ? 'Khẩn' : 'Bình thường';
              const statusText = getDeadlineStatusInfo(task).text;
              return `[${priorityText}] ${task.title} - ${statusText}`;
            }).join('\n');
            
            if (res.data.length > 8) {
              bodyText += `\nvà ${res.data.length - 8} việc khác`;
            }

            const notification = new Notification('Việc cần chú ý hôm nay', {
              body: bodyText,
              requireInteraction: true
            });

            notification.onclick = () => {
              window.focus();
              notification.close();
            };
            
            const updatedIds = [...storedIds, ...newTasks.map(t => t.taskId)];
            localStorage.setItem(storageKey, JSON.stringify(updatedIds));
          }
        }
      } catch (error) {
        console.error('Error fetching alerts for notification:', error);
      }
    };

    const prevDay = () => {
      const [y, m, d] = currentDate.value.split('-');
      const date = new Date(y, m - 1, d);
      date.setDate(date.getDate() - 1);
      currentDate.value = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
      loadTasks();
    };

    const nextDay = () => {
      const [y, m, d] = currentDate.value.split('-');
      const date = new Date(y, m - 1, d);
      date.setDate(date.getDate() + 1);
      currentDate.value = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
      loadTasks();
    };

    const goToToday = () => {
      currentDate.value = defaultDate;
      loadTasks();
    };

    const handleToggleDone = async (task) => {
      // Optimistic update
      task.isDone = !task.isDone;
      try {
        await toggleTaskDone(task.taskId, currentDate.value);
        await loadTasks();
      } catch (error) {
        // Revert on error
        task.isDone = !task.isDone;
        console.error('Error toggling task:', error);
      }
    };

    const handleDelete = async (id) => {
      if (confirm('Bạn có chắc chắn muốn xoá công việc này không?')) {
        try {
          await deleteTask(id);
          await loadTasks();
        } catch (error) {
          console.error('Error deleting task:', error);
        }
      }
    };

    const submitTask = async () => {
      const ddt = newTask.value.deadlineDateTime || '';
      let dDate = '';
      let dTime = null;

      if (ddt) {
        if (ddt.includes('T')) {
          const parts = ddt.split('T');
          dDate = parts[0];
          dTime = parts[1];
        } else {
          dDate = ddt;
        }
      }

      const effectiveStartDate = newTask.value.startDate || dDate;
      if (effectiveStartDate > dDate) {
        alert('Ngày bắt đầu không được sau ngày hết hạn!');
        return;
      }

      try {
        if (!isEditing.value && newTask.value.isRecurring && effectiveStartDate) {
          const [y, m, d] = effectiveStartDate.split('-');
          newTask.value.recurrenceDayOfWeek = new Date(y, m - 1, d).getDay();
        } else {
          newTask.value.recurrenceDayOfWeek = null;
        }

        const payload = {
          title: newTask.value.title,
          note: newTask.value.note,
          categoryId: newTask.value.categoryId,
          priority: newTask.value.priority,
          deadlineDate: dDate,
          deadlineTime: dTime,
          startDate: newTask.value.startDate || null,
          isRecurring: newTask.value.isRecurring,
          recurrenceDayOfWeek: newTask.value.recurrenceDayOfWeek
        };
        
        if (isEditing.value) {
          payload.isDone = newTask.value.isDone;
          await updateTask(editTaskId.value, payload);
        } else {
          await createTask(payload);
          if (payload.isRecurring) {
            alert('Đã tạo 8 công việc lặp lại hàng tuần');
          }
        }
        
        await loadTasks();
        closeModal();
      } catch (error) {
        console.error('Error saving task:', error);
        alert('Có lỗi xảy ra khi lưu công việc!');
      }
    };

    const openViewModal = (task) => {
      isReadOnly.value = true;
      isEditing.value = false;
      editTaskId.value = task.taskId;
      
      let ddt = task.deadlineDate.split('T')[0];
      if (task.deadlineTime) {
         const hm = task.deadlineTime.split(':').slice(0, 2).join(':');
         ddt += `T${hm}`;
      }

      newTask.value = {
        title: task.title,
        note: task.note,
        categoryId: task.categoryId,
        priority: task.priority,
        deadlineDateTime: ddt,
        startDate: task.startDate || currentDate.value,
        isRecurring: task.isRecurring,
        recurrenceDayOfWeek: task.recurrenceDayOfWeek,
        isDone: task.isDone
      };
      showModal.value = true;
    };

    const switchToEdit = () => {
      isReadOnly.value = false;
      isEditing.value = true;
    };

    const openAddModal = () => {
      isEditing.value = false;
      isReadOnly.value = false;
      editTaskId.value = null;
      newTask.value = { 
        ...initialForm, 
        categoryId: categories.value.length ? categories.value[0].categoryId : null,
        startDate: currentDate.value
      };
      showModal.value = true;
    };

    const closeModal = () => {
      showModal.value = false;
      newTask.value = { ...initialForm, categoryId: categories.value.length ? categories.value[0].categoryId : null };
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        checkAlertsForNotification();
      }
    };

    const scheduleMidnightCheck = () => {
      const now = new Date();
      const nextMidnight = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1, 0, 0, 0, 0);
      const msUntilMidnight = nextMidnight.getTime() - now.getTime();

      midnightTimeout = setTimeout(() => {
        checkAlertsForNotification();
        scheduleMidnightCheck();
      }, msUntilMidnight);
    };

    onMounted(() => {
      loadData();
      requestNotificationPermission();
      document.addEventListener('visibilitychange', handleVisibilityChange);
      scheduleMidnightCheck();
    });

    onUnmounted(() => {
      if (notificationInterval) {
        clearInterval(notificationInterval);
      }
      if (midnightTimeout) {
        clearTimeout(midnightTimeout);
      }
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    });

    return {
      tasks,
      filteredAndSortedTasks,
      filterCategory,
      filterPriority,
      categories,
      showModal,
      isEditing,
      isReadOnly,
      editTaskId,
      currentDate,
      defaultDate,
      newTask,
      formattedDate,
      getCategoryName,
      getCategoryColor,
      recurringDayText,
      formatDateStr,
      formatDeadlineStr,
      getDeadlineStatusInfo,
      handleToggleDone,
      handleDelete,
      submitTask,
      openViewModal,
      switchToEdit,
      openAddModal,
      closeModal,
      prevDay,
      nextDay,
      goToToday
    };
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.day-view-container {
  font-family: 'Inter', sans-serif;
  max-width: 800px;
  margin: 40px auto;
  padding: 30px;
  background: #ffffff;
  border-radius: 20px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.05);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  border-bottom: 2px solid #f0f0f5;
  padding-bottom: 20px;
}

.filters-container {
  display: flex;
  gap: 16px;
  margin-bottom: 20px;
}

.filters-container .filter-group {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.filters-container label {
  font-size: 13px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 6px;
}

.date-controls {
  display: flex;
  align-items: center;
  gap: 12px;
}

.date-title {
  font-size: 24px;
  font-weight: 700;
  color: #1a1a2e;
  margin: 0;
  min-width: 330px;
  text-align: center;
}

.nav-btn {
  background: none;
  border: 1px solid #e2e8f0;
  color: #64748b;
  border-radius: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s;
}

.nav-btn:hover {
  background: #f1f5f9;
  color: #334155;
}

.today-btn {
  background: #f1f5f9;
  color: #4f46e5;
  border: none;
  padding: 6px 12px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
}

.today-btn:hover {
  background: #e0e7ff;
}

.add-btn {
  background: #4f46e5;
  color: white;
  border: none;
  width: 44px;
  height: 44px;
  border-radius: 50%;
  font-size: 28px;
  font-weight: 500;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.add-btn:hover {
  transform: translateY(-2px) scale(1.05);
  background: #4338ca;
  box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
}

.add-btn:active {
  transform: translateY(1px);
}

.task-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.task-item {
  display: flex;
  align-items: flex-start;
  background: #f8fafc;
  border-radius: 12px;
  padding: 12px 16px;
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
}

.task-item.clickable {
  cursor: pointer;
}

.task-item:hover {
  background: #f1f5f9;
  transform: translateX(4px);
}

.category-badge {
  font-size: 11px;
  padding: 4px 8px;
  border-radius: 6px;
  font-weight: 600;
  margin-right: 12px;
  white-space: nowrap;
  margin-top: 2px;
}

.task-content {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  flex: 1;
  min-width: 0; /* needed for truncate to work */
}

.task-checkbox {
  width: 20px;
  height: 20px;
  cursor: pointer;
  accent-color: #4f46e5;
  flex-shrink: 0;
  margin-top: 2px;
}

.task-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow: hidden;
  min-width: 0;
}

.task-title {
  font-size: 16px;
  color: #334155;
  font-weight: 500;
  transition: all 0.3s ease;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: left;
}

.task-note-preview {
  font-size: 13px;
  color: #64748b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  text-align: left;
}

.task-actions {
  display: flex;
  align-items: flex-start;
  gap: 12px;
}

.badge-group {
  display: grid;
  grid-template-columns: 130px 105px;
  gap: 8px;
  align-items: start;
}

.priority-label {
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  background: #e2e8f0;
  color: #64748b;
  justify-self: start;
  min-width: 100px;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  white-space: nowrap;
}

.priority-label.urgent {
  background: #fee2e2;
  color: #ef4444;
}

.deadline-label {
  font-size: 12px;
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  justify-self: start;
  width: 130px;
  box-sizing: border-box;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  white-space: nowrap;
}

.deadline-label.normal {
  background: #f1f5f9;
  color: #64748b;
}

.deadline-label.today {
  background: #fef08a;
  color: #a16207;
}

.deadline-label.overdue {
  background: #ffedd5;
  color: #ea580c;
}

.delete-btn {
  background: none;
  border: none;
  color: #cbd5e1;
  cursor: pointer;
  padding: 6px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.delete-btn:hover {
  color: #ef4444;
  background: #fee2e2;
}

/* Done state */
.task-item.is-done {
  opacity: 0.6;
}
.task-item.is-done .task-title {
  text-decoration: line-through;
  color: #94a3b8;
}
.task-item.is-done .priority-label {
  filter: grayscale(100%);
  opacity: 0.7;
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: #94a3b8;
  font-style: italic;
  font-size: 15px;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.2s ease;
}

.modal-content {
  background: white;
  width: 90%;
  max-width: 500px;
  border-radius: 24px;
  padding: 32px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.1);
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-content h2 {
  margin-top: 0;
  margin-bottom: 24px;
  font-size: 20px;
  color: #1e293b;
}

.form-group {
  margin-bottom: 16px;
  display: flex;
  flex-direction: column;
}

.form-row {
  display: flex;
  gap: 16px;
}
.form-row .form-group {
  flex: 1;
}

.form-group label {
  font-size: 13px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 6px;
}

.required {
  color: #ef4444;
}

input, select, textarea {
  font-family: inherit;
  padding: 10px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  color: #334155;
  transition: border-color 0.2s;
  outline: none;
}

input:focus, select:focus, textarea:focus {
  border-color: #4f46e5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 32px;
}

.btn-cancel {
  padding: 10px 20px;
  background: #f1f5f9;
  color: #475569;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-cancel:hover {
  background: #e2e8f0;
}

.btn-submit {
  padding: 10px 24px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-submit:hover {
  background: #4338ca;
}

.read-only-details {
  display: flex;
  flex-direction: column;
  margin-bottom: 20px;
}

.detail-row {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  align-items: flex-start;
  gap: 6px;
  padding: 14px 0;
  border-bottom: 1px solid #f1f5f9;
  width: 100%;
  box-sizing: border-box;
}

.detail-row:last-of-type {
  border-bottom: none;
}

.detail-label {
  font-size: 13px;
  color: #94a3b8;
  font-weight: 500;
  flex-shrink: 0;
  width: auto;
}

.detail-value {
  font-size: 14px;
  color: #1e293b;
  font-weight: 500;
  text-align: left;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 8px;
  min-width: 0;
}

.detail-value.note-value {
  text-align: left;
  white-space: pre-wrap;
  font-weight: 400;
  color: #475569;
  word-wrap: break-word;
  word-break: break-all;
  overflow-wrap: anywhere;
}

.category-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  display: inline-block;
  flex-shrink: 0;
}

.urgent-text {
  color: #ef4444 !important;
  font-weight: 600;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
