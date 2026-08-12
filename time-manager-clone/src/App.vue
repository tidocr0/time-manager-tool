<script setup>
import DayView from './components/DayView.vue'
</script>

<template>
  <div class="app-background">
    <DayView />
  </div>
</template>

<style>
body {
  margin: 0;
  padding: 0;
  background-color: #f1f5f9;
}
.app-background {
  min-height: 100vh;
  padding: 20px;
}
</style>
