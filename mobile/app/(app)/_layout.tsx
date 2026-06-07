import { Stack } from "expo-router";

export default function AppLayout() {
  return (
    <Stack screenOptions={{ headerShown: true, headerBackTitle: "Back" }}>
      <Stack.Screen name="admin/index" options={{ title: "Admin" }} />
      <Stack.Screen name="admin/whatsapp" options={{ title: "WhatsApp" }} />
      <Stack.Screen name="teacher/index" options={{ title: "Teacher" }} />
      <Stack.Screen name="parent/index" options={{ title: "Parent" }} />
    </Stack>
  );
}
