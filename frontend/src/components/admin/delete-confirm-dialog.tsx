"use client";

import { useEffect, useRef } from "react";
import { Loader2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export type DeleteConfirmState = {
  id: string;
  description: string;
} | null;

type DeleteConfirmDialogProps = {
  state: DeleteConfirmState;
  onOpenChange: (open: boolean) => void;
  onConfirm: (id: string) => void;
  isPending?: boolean;
};

export function DeleteConfirmDialog({
  state,
  onOpenChange,
  onConfirm,
  isPending = false,
}: DeleteConfirmDialogProps) {
  const t = useTranslations("common.deleteDialog");
  const confirmIdRef = useRef<string | null>(null);

  useEffect(() => {
    if (state?.id) confirmIdRef.current = state.id;
  }, [state?.id]);

  const handleConfirm = () => {
    const id = state?.id ?? confirmIdRef.current;
    if (id && !isPending) onConfirm(id);
  };

  return (
    <Dialog
      open={state !== null}
      onOpenChange={(open) => {
        if (!open && isPending) return;
        onOpenChange(open);
      }}
    >
      <DialogContent
        className="max-w-md"
        onPointerDownOutside={(e) => {
          if (isPending) e.preventDefault();
        }}
        onEscapeKeyDown={(e) => {
          if (isPending) e.preventDefault();
        }}
      >
        <DialogHeader>
          <DialogTitle>{t("title")}</DialogTitle>
          <DialogDescription>{state?.description}</DialogDescription>
        </DialogHeader>
        <DialogFooter className="gap-2 sm:gap-0">
          <Button
            type="button"
            variant="outline"
            disabled={isPending}
            onClick={() => onOpenChange(false)}
          >
            {t("cancel")}
          </Button>
          <Button
            type="button"
            variant="destructive"
            disabled={isPending}
            onClick={handleConfirm}
          >
            {isPending ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                {t("deleting")}
              </>
            ) : (
              t("confirm")
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
